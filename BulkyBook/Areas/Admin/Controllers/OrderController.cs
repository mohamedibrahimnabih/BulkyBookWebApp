using BulkyBook.DataAccess.Repository;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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
                OrderHeader = unitOfWork.OrderHeaderRepository.GetOne(e => e.Id == id, "ApplicationUser")
            };

            if (OrderVM.OrderHeader != null)
            {
                OrderVM.OrderDetails = unitOfWork.OrderDetailRepository.Get(e => e.OrderHeaderId == id, "Product");
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
				orderHeaderFromDb.Carrier = OrderVM.OrderHeader.Carrier;
				orderHeaderFromDb.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
				unitOfWork.OrderHeaderRepository.Update(orderHeaderFromDb);
				unitOfWork.Commit();

				TempData["alert"] = "Order Details Updated Successfully.";

				return RedirectToAction(nameof(Details), new { id = orderHeaderFromDb.Id });
			}

            return NotFound();
		}

		#region APIs
		[HttpGet]
		public IActionResult GetAll(string status)
        {
            IEnumerable<OrderHeader> orderHeaders;

			if (User.IsInRole(StaticData.Role_Admin) || User.IsInRole(StaticData.Role_Employee))
			{
				if (status == "All" || string.IsNullOrEmpty(status))
					orderHeaders = unitOfWork.OrderHeaderRepository.GetAll(includeProperties: "ApplicationUser");
				else
					orderHeaders = unitOfWork.OrderHeaderRepository.Get(
						expression: o => o.OrderStatus == status || o.PaymentStatus == status,
						includeProperties: "ApplicationUser");
			}
			else
			{
				var userId = _userManager.GetUserId(User);
				if (userId != null)
				{
					if (status == "All" || string.IsNullOrEmpty(status))
						orderHeaders = unitOfWork.OrderHeaderRepository.Get(e => e.ApplicationUserId == userId, includeProperties: "ApplicationUser");
					else
						orderHeaders = unitOfWork.OrderHeaderRepository.Get(
							expression: o => ((o.OrderStatus == status || o.PaymentStatus == status) && o.ApplicationUserId == userId),
							includeProperties: "ApplicationUser");

				}
				else
					return NotFound();
			}

			return Json(orderHeaders);
        }
        #endregion
    }
}
