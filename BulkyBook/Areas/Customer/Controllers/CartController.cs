using BulkyBook.DataAccess.Repository;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

namespace BulkyBook.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;

        public CartController(IUnitOfWork unitOfWork, UserManager<IdentityUser> userManager)
        {
            this.unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var userId = _userManager.GetUserId(User);

            if (userId != null)
            {
                var cart = unitOfWork.ShoppingCartRepository.Get(e => e.ApplicationUserId == userId, includeProperties: "Product");
                
                var shoppingCartVM = new ShoppingCartVM
                {
                    CartItems = cart,
                    OrderHeader = new() { OrderTotal = CalcOrderTotal(cart) }
                };

                return View(shoppingCartVM);
            }

            return NotFound();
        }

        public IActionResult Summary()
        {
            var userId = _userManager.GetUserId(User);

            if (userId != null)
            {
                var cart = unitOfWork.ShoppingCartRepository.Get(e => e.ApplicationUserId == userId, includeProperties: "Product");
                var user = unitOfWork.ApplicationUserRepository.GetOne(e => e.Id == userId);

                var shoppingCartVM = new ShoppingCartVM
                {
                    CartItems = cart,
                    OrderHeader = new()
                    {
                        OrderTotal = CalcOrderTotal(cart),
                        Name = user.Name,
                        PhoneNumber = user.PhoneNumber,
                        StreetAddress = user.StreetAddress,
                        City = user.City,
                        State = user.State,
                        PostalCode = user.ZipCode
                    }
                };

                return View(shoppingCartVM);
            }

            return NotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Summary(ShoppingCartVM shoppingCartVM)
        {
            // ApplicationUserId
            var userId = _userManager.GetUserId(User);
            if (userId != null)
            {
                shoppingCartVM.OrderHeader.ApplicationUserId = userId;

                // Remove the existing model state error for ApplicationUserId
                ModelState.Remove("OrderHeader.ApplicationUserId");

                // Ensure ApplicationUserId is set before validating the model state
                //TryValidateModel(shoppingCartVM);
            }

            // Fill CartItems
            var cart = unitOfWork.ShoppingCartRepository.Get(e => e.ApplicationUserId == userId, includeProperties: "Product");
            if (cart != null)
            {
                shoppingCartVM.CartItems = cart;
                ModelState.Remove("CartItems");
            }

            if (ModelState.IsValid)
            {
                // Update Profile Data
                var user = unitOfWork.ApplicationUserRepository.GetOne(e => e.Id == userId, tracked: true);
                if (shoppingCartVM.OrderHeader.UpdateProfileData)
                {
                    user.Name = shoppingCartVM.OrderHeader.Name;
                    user.PhoneNumber = shoppingCartVM.OrderHeader.PhoneNumber;
                    user.StreetAddress = shoppingCartVM.OrderHeader.StreetAddress;
                    user.City = shoppingCartVM.OrderHeader.City;
                    user.State = shoppingCartVM.OrderHeader.State;
                    user.ZipCode = shoppingCartVM.OrderHeader.PostalCode;
                }
                shoppingCartVM.OrderHeader.ApplicationUser = user;

                // Fill OrderHeader
                shoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
                shoppingCartVM.OrderHeader.OrderTotal = CalcOrderTotal(cart);
                if (shoppingCartVM.OrderHeader.ApplicationUser.CompanyId.GetValueOrDefault() == 0)
                {
                    // Regular Customer
                    shoppingCartVM.OrderHeader.OrderStatus = StaticData.StatusPending;
                    shoppingCartVM.OrderHeader.PaymentStatus = StaticData.PaymentStatusPending;
                }
                else
                {
                    // Company
                    shoppingCartVM.OrderHeader.OrderStatus = StaticData.StatusApproved;
                    shoppingCartVM.OrderHeader.PaymentStatus = StaticData.PaymentStatusDelayedPayment;
                }
                unitOfWork.OrderHeaderRepository.Add(shoppingCartVM.OrderHeader);
                unitOfWork.Commit();

                // Fill OrderDetail
                List<OrderDetail> orderDetails = new();
                foreach (var item in shoppingCartVM.CartItems)
                {
                    OrderDetail orderDetail = new()
                    {
                        OrderHeaderId = shoppingCartVM.OrderHeader.Id,
                        ProductId = item.ProductId,
                        Count = item.Count,
                        Price =
                        item.Count <= 50 ? item.Product.Price :
                        item.Count <= 100 ? item.Product.Price50 :
                        item.Product.Price100
                    };

                    orderDetails.Add(orderDetail);
                }
                unitOfWork.OrderDetailRepository.AddRange(orderDetails);
                unitOfWork.Commit();

                // Configure Stripe, Complete Payment
                if (shoppingCartVM.OrderHeader.ApplicationUser.CompanyId.GetValueOrDefault() == 0)
                {
                    var options = new SessionCreateOptions
                    {
                        PaymentMethodTypes = new List<string> { "card" },
                        LineItems = new List<SessionLineItemOptions>(),
                        Mode = "payment",
                        SuccessUrl = $"{GetDomainName()}/Customer/Cart/CompleteOrder/{shoppingCartVM.OrderHeader.Id}",
                        CancelUrl = $"{GetDomainName()}/Customer/Cart/PaymentIssue",
                    };

                    foreach (var item in shoppingCartVM.CartItems)
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
                                UnitAmount = (long)item.Product.Price * 100,
                            }
                        };
                        options.LineItems.Add(lineItem);
                    }

                    var service = new SessionService();
                    var session = service.Create(options);
                    shoppingCartVM.OrderHeader.SessionId = session.Id;
                    unitOfWork.Commit();

                    return Redirect(session.Url);
                }
                    
                return RedirectToAction(nameof(CompleteOrder), new { id = shoppingCartVM.OrderHeader.Id });
            }

            return View(shoppingCartVM);
        }

        public IActionResult CompleteOrder(int id)
        {
            var order = unitOfWork.OrderHeaderRepository.GetOne(e => e.Id == id, tracked: true);
            if(order != null)
            {
                if(order.PaymentStatus != StaticData.PaymentStatusDelayedPayment)
                {
                    var service = new SessionService();
                    var session = service.Get(order.SessionId);

                    if (session.PaymentStatus == "paid")
                    {
                        order.OrderStatus = StaticData.StatusApproved;
                        order.PaymentStatus = StaticData.PaymentStatusApproved;
                        order.PaymentDate = DateTime.Now;
                        order.PaymentIntentId = session.PaymentIntentId;

                        // Remove the shopping cart
                        var userId = _userManager.GetUserId(User);
                        if (userId != null)
                        {
                            var cart = unitOfWork.ShoppingCartRepository.Get(e => e.ApplicationUserId == userId);
                            unitOfWork.ShoppingCartRepository.RemoveRange(cart);
                        }

                        unitOfWork.Commit();
                        return View(id);
                    }
                }
            }

            return View(nameof(PaymentIssue));
        }

        public IActionResult PaymentIssue()
        {
            return View();
        }

        private double CalcOrderTotal(IEnumerable<ShoppingCart> carts)
        {
            var total = carts.Sum(item =>
                    item.Count <= 50 ? item.Product.Price * item.Count :
                    item.Count <= 100 ? item.Product.Price50 * item.Count :
                    item.Product.Price100 * item.Count
                );

            return total;
        }

        private string GetDomainName()
        {
            var request = HttpContext.Request;
            var domainName = $"{request.Scheme}://{request.Host}";
            return domainName;
        }

        #region Increment, Decrement and Remove
        public IActionResult IncrementCount(int cartId)
        {
            var cart = unitOfWork.ShoppingCartRepository.GetOne(c => c.Id == cartId, tracked: true);
            if (cart != null)
            {
                cart.Count += 1;
                unitOfWork.Commit();
            }
            return RedirectToAction(nameof(Index));
        }

        public IActionResult DecrementCount(int cartId)
        {
            var cart = unitOfWork.ShoppingCartRepository.GetOne(c => c.Id == cartId, tracked: true);
            if (cart != null)
            {
                if (cart.Count > 1)
                {
                    cart.Count -= 1;
                    unitOfWork.Commit();
                }
                else
                {
                    unitOfWork.ShoppingCartRepository.Remove(cart);
                    unitOfWork.Commit();
                }
            }
            return RedirectToAction(nameof(Index));
        }

        public IActionResult RemoveFromCart(int cartId)
        {
            var cart = unitOfWork.ShoppingCartRepository.GetOne(c => c.Id == cartId, tracked: true);
            if (cart != null)
            {
                unitOfWork.ShoppingCartRepository.Remove(cart);
                unitOfWork.Commit();
            }
            return RedirectToAction(nameof(Index));
        }
        #endregion
    }
}
