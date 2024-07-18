using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BulkyBook.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork, UserManager<IdentityUser> userManager)
        {
            _logger = logger;
            this.unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public IActionResult Index() => View(unitOfWork.ProductRepository.GetAll(includeProperties: "Category"));

        public IActionResult Details(int? id)
        {
            var cart = new ShoppingCart();
            if(id != null)
            {
                cart.Product = unitOfWork.ProductRepository.GetOne(e => e.Id == id, includeProperties: "Category");
                cart.Count = 1;
                cart.ProductId = (int)id;
            }

            return cart.Product != null ? View(cart) : NotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Details(ShoppingCart cart)
        {
            var userId = _userManager.GetUserId(User);
            if (userId != null)
            {
                cart.ApplicationUserId = userId;

                // Remove the existing model state error for ApplicationUserId
                ModelState.Remove("ApplicationUserId");

                // Ensure ApplicationUserId is set before validating the model state
                //TryValidateModel(cart);
            }

            if (ModelState.IsValid)
            {
                var existingCart = unitOfWork.ShoppingCartRepository
                    .GetOne(e => e.ApplicationUserId == cart.ApplicationUserId && e.ProductId == cart.ProductId);

                if (existingCart != null)
                {
                    existingCart.Count += cart.Count;
                    unitOfWork.ShoppingCartRepository.Update(existingCart);
                }
                else
                {
                    cart.Id = 0;
                    unitOfWork.ShoppingCartRepository.Add(cart);
                }

                unitOfWork.Commit();
                TempData["alert"] = "Added items successfully";
                return RedirectToAction(nameof(Index));
            }

            cart.Product = unitOfWork.ProductRepository.GetOne(e => e.Id == cart.ProductId, includeProperties: "Category");
            return View(cart);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
