using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models.ViewModels;
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
                var total = cart.Sum(item =>
                    item.Count <= 50 ? item.Product.Price * item.Count :
                    item.Count <= 100 ? item.Product.Price50 * item.Count :
                    item.Product.Price100 * item.Count
                );

                var shoppingCartVM = new ShoppingCartVM
                {
                    CartItems = cart,
                    OrderHeader = new() { OrderTotal = total }
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
                var total = cart.Sum(item =>
                    item.Count <= 50 ? item.Product.Price * item.Count :
                    item.Count <= 100 ? item.Product.Price50 * item.Count :
                    item.Product.Price100 * item.Count
                );

                var user = unitOfWork.ApplicationUserRepository.GetOne(e => e.Id == userId);

                var shoppingCartVM = new ShoppingCartVM
                {
                    CartItems = cart,
					OrderHeader = new()
                    {
                        OrderTotal = total,
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
