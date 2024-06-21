using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyBook.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork unitOfWork;

        public ProductController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public IActionResult Index() => View(unitOfWork.ProductRepository.GetAll());

        public IActionResult Create()
        {
            IEnumerable<SelectListItem> ListOfCategories = unitOfWork.CategoryRepository.GetAll().Select(e => new SelectListItem
            {
                Text = e.Name,
                Value = e.Id.ToString()
            });

            //ViewData["ListOfCategories"] = ListOfCategories;

            ProductVM productVM = new ProductVM()
            {
                ListOfCategories = ListOfCategories
            };

            return View(productVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Product product)
        {
            if (ModelState.IsValid)
            {
                unitOfWork.ProductRepository.Add(product);
                unitOfWork.Commit();

                TempData["alert"] = "Added successfully";

                return RedirectToAction("Index");
            }

            //ViewData["ListOfCategories"] = ListOfCategories;

            ProductVM productVM = new ProductVM()
            {
                ListOfCategories = unitOfWork.CategoryRepository.GetAll().Select(e => new SelectListItem
                {
                    Text = e.Name,
                    Value = e.Id.ToString()
                })
            };

            return View(productVM);
        }

        public IActionResult Edit(int id)
        {
            var product = unitOfWork.ProductRepository.GetOne(e => e.Id == id);

            return product != null ? View(product) : NotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Product product)
        {
            if (ModelState.IsValid)
            {
                unitOfWork.ProductRepository.Update(product);
                unitOfWork.Commit();

                TempData["alert"] = "Edited successfully";

                return RedirectToAction("Index");
            }

            return View();
        }

        public IActionResult Delete(int id)
        {
            var product = unitOfWork.ProductRepository.GetOne(e => e.Id == id);

            if (product != null)
            {
                unitOfWork.ProductRepository.Remove(product);
                unitOfWork.Commit();

                TempData["alert"] = "Deleted successfully";

                return RedirectToAction("Index");
            }

            return NotFound();
        }
    }
}
