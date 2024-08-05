using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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

                return RedirectToAction(nameof(CompleteOrder), new { id = shoppingCartVM.OrderHeader.Id });
            }

            return View(shoppingCartVM);
        }

        public IActionResult CompleteOrder(int id)
        {
            return View(id);
        }

        public double CalcOrderTotal(IEnumerable<ShoppingCart> carts)
        {
            var total = carts.Sum(item =>
                    item.Count <= 50 ? item.Product.Price * item.Count :
                    item.Count <= 100 ? item.Product.Price50 * item.Count :
                    item.Product.Price100 * item.Count
                );

            return total;
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
