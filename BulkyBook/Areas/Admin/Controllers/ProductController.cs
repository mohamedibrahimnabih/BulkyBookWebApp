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

        public IActionResult UpSert(int? id)
        {
            IEnumerable<SelectListItem> ListOfCategories = unitOfWork.CategoryRepository.GetAll().Select(e => new SelectListItem
            {
                Text = e.Name,
                Value = e.Id.ToString()
            });

            //ViewData["ListOfCategories"] = ListOfCategories;

            ProductVM productVM = new ProductVM()
            {
                ListOfCategories = ListOfCategories,
                Product = new Product()
            };

            if (id != null)
            {
                productVM.Product = unitOfWork.ProductRepository.GetOne(e => e.Id == id);
            }

            return productVM.Product != null ? View(productVM) : NotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpSert(ProductVM productVM)
        {
            if (ModelState.IsValid)
            {
                if (productVM.Product.Id == 0)
                {
                    unitOfWork.ProductRepository.Add(productVM.Product);

                    TempData["alert"] = "Added successfully";
                }
                else
                {
                    unitOfWork.ProductRepository.Update(productVM.Product);

                    TempData["alert"] = "Edited successfully";
                }

                unitOfWork.Commit();
                return RedirectToAction("Index");
            }

            productVM.ListOfCategories = unitOfWork.CategoryRepository.GetAll().Select(e => new SelectListItem
            {
                Text = e.Name,
                Value = e.Id.ToString()
            });

            return View(productVM);
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
