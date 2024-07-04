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
        private readonly IWebHostEnvironment webHostEnvironment;

        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            this.unitOfWork = unitOfWork;
            this.webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index() => View(unitOfWork.ProductRepository.GetAll(includeProperties: "Category"));

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
        public async Task<IActionResult> UpSert(ProductVM productVM, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                // Handle file
                if (file != null && file.Length > 0)
                {
                    string imagesFolderPath = Path.Combine(webHostEnvironment.WebRootPath, "images/products");
                    Directory.CreateDirectory(imagesFolderPath);

                    if (!string.IsNullOrEmpty(productVM.Product.ImgURL))
                    {
                        string oldImagePath = Path.Combine(webHostEnvironment.WebRootPath, productVM.Product.ImgURL.TrimStart('/'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName); // Generate a new guid and use it as the file name
                    string filePath = Path.Combine(imagesFolderPath, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }

                    productVM.Product.ImgURL = "/images/products/" + fileName;
                }

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
                var imagePath = product.ImgURL;

                unitOfWork.ProductRepository.Remove(product);
                unitOfWork.Commit();

                if (!string.IsNullOrEmpty(imagePath))
                {
                    string fullPath = Path.Combine(webHostEnvironment.WebRootPath, imagePath.TrimStart('/'));
                    if (System.IO.File.Exists(fullPath))
                    {
                        System.IO.File.Delete(fullPath);
                    }
                }

                TempData["alert"] = "Deleted successfully";

                return RedirectToAction("Index");
            }

            return NotFound();
        }

        #region APIs
        [HttpGet]
        public IActionResult GetAll() => Json(unitOfWork.ProductRepository.GetAll(includeProperties: "Category"));
        #endregion
    }
}
