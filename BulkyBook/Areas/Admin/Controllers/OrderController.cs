using BulkyBook.DataAccess.Repository;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;

namespace BulkyBook.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork unitOfWork;
		private readonly UserManager<IdentityUser> _userManager;

		[BindProperty]
        public OrderVM OrderVM { get; set; }

        public OrderController(IUnitOfWork unitOfWork, UserManager<IdentityUser> userManager)
        {
            this.unitOfWork = unitOfWork;
			this._userManager = userManager;
		}

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(int id)
        {
            OrderVM = new()
            {
                OrderHeader = unitOfWork.OrderHeaderRepository.GetOne(e => e.Id == id, includeProperties: e => e.ApplicationUser)
            };

            if (OrderVM.OrderHeader != null)
            {
                OrderVM.OrderDetails = unitOfWork.OrderDetailRepository.Get(e => e.OrderHeaderId == id, includeProperties: e => e.Product);
                return View(OrderVM);
            }
            
            return NotFound();
        }

        [HttpPost]
        [Authorize(Roles = ($"{StaticData.Role_Admin},{StaticData.Role_Employee}"))]
        [ValidateAntiForgeryToken]
		public IActionResult UpdateOrderDetail()
		{
			var orderHeaderFromDb = unitOfWork.OrderHeaderRepository.GetOne(u => u.Id == OrderVM.OrderHeader.Id);
			if(orderHeaderFromDb != null)
            {
				orderHeaderFromDb.Name = OrderVM.OrderHeader.Name;
				orderHeaderFromDb.PhoneNumber = OrderVM.OrderHeader.PhoneNumber;
				orderHeaderFromDb.StreetAddress = OrderVM.OrderHeader.StreetAddress;
				orderHeaderFromDb.City = OrderVM.OrderHeader.City;
				orderHeaderFromDb.State = OrderVM.OrderHeader.State;
				orderHeaderFromDb.PostalCode = OrderVM.OrderHeader.PostalCode;
				if(OrderVM.OrderHeader.Carrier != null)
					orderHeaderFromDb.Carrier = OrderVM.OrderHeader.Carrier;
                if (OrderVM.OrderHeader.TrackingNumber != null)
                    orderHeaderFromDb.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;

                unitOfWork.OrderHeaderRepository.Update(orderHeaderFromDb);
				unitOfWork.Commit();

				TempData["alert"] = "Order Details Updated Successfully.";

				return RedirectToAction(nameof(Details), new { id = OrderVM.OrderHeader.Id });
			}

            return NotFound();
		}

		[HttpPost]
		[Authorize(Roles = ($"{StaticData.Role_Admin},{StaticData.Role_Employee}"))]
		[ValidateAntiForgeryToken]
		public IActionResult StartProcessing()
		{
            var orderHeaderFromDb = unitOfWork.OrderHeaderRepository.GetOne(u => u.Id == OrderVM.OrderHeader.Id);
            if (orderHeaderFromDb != null)
            {
                orderHeaderFromDb.OrderStatus = StaticData.StatusInProcess;
                unitOfWork.OrderHeaderRepository.Update(orderHeaderFromDb);
                unitOfWork.Commit();

                TempData["alert"] = "Order Details Updated Successfully.";

                return RedirectToAction(nameof(Details), new { id = OrderVM.OrderHeader.Id });
            }

            return NotFound();
        }

        [HttpPost]
        [Authorize(Roles = ($"{StaticData.Role_Admin},{StaticData.Role_Employee}"))]
        [ValidateAntiForgeryToken]
        public IActionResult ShipOrder()
        {
            var orderHeaderFromDb = unitOfWork.OrderHeaderRepository.GetOne(u => u.Id == OrderVM.OrderHeader.Id, includeProperties: e => e.ApplicationUser);
            if (orderHeaderFromDb != null)
            {
                orderHeaderFromDb.OrderStatus = StaticData.StatusShipped;
                orderHeaderFromDb.ShippingDate = DateTime.Now;
                if (orderHeaderFromDb.Carrier == null)
                    orderHeaderFromDb.Carrier = OrderVM.OrderHeader.Carrier;
                if (orderHeaderFromDb.TrackingNumber == null)
                    orderHeaderFromDb.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
                if(orderHeaderFromDb.ApplicationUser.CompanyId.GetValueOrDefault() != 0)
                    orderHeaderFromDb.PaymentDueDate = DateTime.Now.AddDays(30);
                unitOfWork.OrderHeaderRepository.Update(orderHeaderFromDb);
                unitOfWork.Commit();

                TempData["alert"] = "Order Details Updated Successfully.";

                return RedirectToAction(nameof(Details), new { id = OrderVM.OrderHeader.Id });
            }

            return NotFound();
        }

        [HttpPost]
        [Authorize(Roles = $"{StaticData.Role_Admin},{StaticData.Role_Employee}")]
        [ValidateAntiForgeryToken]
        public IActionResult CancelOrder()
        {
            var orderHeaderFromDb = unitOfWork.OrderHeaderRepository.GetOne(u => u.Id == OrderVM.OrderHeader.Id);

            if (orderHeaderFromDb != null)
            {
                if(orderHeaderFromDb.PaymentStatus == StaticData.PaymentStatusApproved)
                {
                    var service = new SessionService();
                    var session = service.Get(orderHeaderFromDb.SessionId);

                    var refundOptions = new RefundCreateOptions
                    {
                        PaymentIntent = session.PaymentIntentId,
                        Amount = session.AmountTotal,
                        Reason = RefundReasons.RequestedByCustomer
                    };

                    var refundService = new RefundService();
                    var refund = refundService.Create(refundOptions);
                    orderHeaderFromDb.PaymentStatus = StaticData.PaymentStatusRefunded;
                }
                
                orderHeaderFromDb.OrderStatus = StaticData.StatusCancelled;
                unitOfWork.OrderHeaderRepository.Update(orderHeaderFromDb);
                unitOfWork.Commit();

                return RedirectToAction(nameof(Details), new { id = OrderVM.OrderHeader.Id });
            }

            return NotFound();
        }

        [HttpPost]
        [Authorize(Roles = $"{StaticData.Role_Admin},{StaticData.Role_Employee},{StaticData.Role_Company}")]
        [ValidateAntiForgeryToken]
        public IActionResult PayNow()
        {
            var orderHeaderFromDb = unitOfWork.OrderHeaderRepository.GetOne(u => u.Id == OrderVM.OrderHeader.Id);
            var OrderDetails = unitOfWork.OrderDetailRepository.Get(e => e.OrderHeaderId == OrderVM.OrderHeader.Id, includeProperties: e => e.Product);

            if (orderHeaderFromDb != null)
            {
                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string> { "card" },
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment",
                    SuccessUrl = $"{GetDomainName()}/Customer/Cart/CompleteOrder/{OrderVM.OrderHeader.Id}",
                    CancelUrl = $"{GetDomainName()}/Customer/Cart/PaymentIssue",
                };

                foreach (var item in OrderDetails)
                {
                    var lineItem = new SessionLineItemOptions
                    {
                        Quantity = item.Count,
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "USD",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.Product.Title
                            },
                            UnitAmount =
                                item.Count <= 50 ? (long)item.Product.Price * 100:
                                item.Count <= 100 ? (long)item.Product.Price50 * 100:
                                (long)item.Product.Price100 * 100
                        }
                    };
                    options.LineItems.Add(lineItem);
                }

                var service = new SessionService();
                var session = service.Create(options);
                orderHeaderFromDb.SessionId = session.Id;
                unitOfWork.OrderHeaderRepository.Update(orderHeaderFromDb);
                unitOfWork.Commit();

                return Redirect(session.Url);
            }

            return NotFound();
        }

        public IActionResult CompleteOrder(int id)
        {
            if (TempData["RedirectedFromCompleteOrder"] == null)
                return NotFound();

            TempData.Remove("RedirectedFromCompleteOrder");
            return View(id);
        }

        private string GetDomainName()
        {
            var request = HttpContext.Request;
            var domainName = $"{request.Scheme}://{request.Host}";
            return domainName;
        }

        #region APIs
        [HttpGet]
		public IActionResult GetAll(string status)
        {
            IEnumerable<OrderHeader> orderHeaders;

			if (User.IsInRole(StaticData.Role_Admin) || User.IsInRole(StaticData.Role_Employee))
			{
				if (status == "All" || string.IsNullOrEmpty(status))
					orderHeaders = unitOfWork.OrderHeaderRepository.Get(includeProperties: e => e.ApplicationUser);
				else
					orderHeaders = unitOfWork.OrderHeaderRepository.Get(
						expression: o => o.OrderStatus == status || o.PaymentStatus == status,
						includeProperties: e => e.ApplicationUser);
			}
			else
			{
				var userId = _userManager.GetUserId(User);
				if (userId != null)
				{
					if (status == "All" || string.IsNullOrEmpty(status))
						orderHeaders = unitOfWork.OrderHeaderRepository.Get(e => e.ApplicationUserId == userId, includeProperties: e => e.ApplicationUser);
					else
						orderHeaders = unitOfWork.OrderHeaderRepository.Get(
							expression: o => ((o.OrderStatus == status || o.PaymentStatus == status) && o.ApplicationUserId == userId),
							includeProperties: e => e.ApplicationUser);

				}
				else
					return NotFound();
			}

			return Json(orderHeaders);
        }
        #endregion
    }
}
