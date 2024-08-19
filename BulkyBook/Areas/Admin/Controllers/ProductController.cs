using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyBook.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = ($"{StaticData.Role_Admin},{StaticData.Role_Employee}"))]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IWebHostEnvironment webHostEnvironment;

        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            this.unitOfWork = unitOfWork;
            this.webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index() => View();

        public IActionResult UpSert(int? id)
        {
            IEnumerable<SelectListItem> ListOfCategories = unitOfWork.CategoryRepository.Get().Select(e => new SelectListItem
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
                productVM.Product = unitOfWork.ProductRepository.GetOne(e => e.Id == id, includeProperties: e => e.ProductImages);
            }

            return productVM.Product != null ? View(productVM) : NotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpSert(ProductVM productVM)
        {
            if (ModelState.IsValid)
            {
                if (productVM.Product.Id == 0)
                {
                    unitOfWork.ProductRepository.Add(productVM.Product);
                    unitOfWork.Commit(); // Commit to get the Id
                    TempData["alert"] = "Added successfully";
                }

                // Handle files
                if (productVM.Files != null && productVM.Files.Any())
                {
                    string productFolderPath = Path.Combine(webHostEnvironment.WebRootPath, "images/products/", $"product-{productVM.Product.Id.ToString()}");
                    Directory.CreateDirectory(productFolderPath);

                    productVM.Product.ProductImages = new List<ProductImage>();
                    foreach (var file in productVM.Files)
                    {
                        if (file.Length > 0)
                        {
                            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                            string filePath = Path.Combine(productFolderPath, fileName);

                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(fileStream);
                            }

                            var productImage = new ProductImage
                            {
                                URL = Path.Combine("/images/products/", $"product-{productVM.Product.Id.ToString()}", fileName).Replace("\\", "/"),
                                ProductId = productVM.Product.Id
                            };

                            productVM.Product.ProductImages.Add(productImage);
                        }
                    }
                    unitOfWork.ProductImageRepository.AddRange(productVM.Product.ProductImages);
                }

                if (productVM.Product.Id != 0)
                {
                    unitOfWork.ProductRepository.Update(productVM.Product);
                    TempData["alert"] = "Edited successfully";
                }

                unitOfWork.Commit();
                return RedirectToAction(nameof(Index));
            }

            productVM.ListOfCategories = unitOfWork.CategoryRepository.Get().Select(e => new SelectListItem
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
                string productFolderPath = Path.Combine(webHostEnvironment.WebRootPath, "images/products/", $"product-{id}");

                if (Directory.Exists(productFolderPath))
                {
                    Directory.Delete(productFolderPath, true);
                }

                unitOfWork.ProductRepository.Remove(product);
                unitOfWork.Commit();

                TempData["alert"] = "Deleted successfully";

                return RedirectToAction(nameof(Index));
            }

            return NotFound();
        }

        public IActionResult DeleteImage(int imageId)
        {
            var productImage = unitOfWork.ProductImageRepository.GetOne(e => e.Id == imageId);
            
            var imagePath = productImage.URL;
            if (!string.IsNullOrEmpty(imagePath))
            {
                string fullPath = Path.Combine(webHostEnvironment.WebRootPath, imagePath.TrimStart('/'));
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }
            }

            unitOfWork.ProductImageRepository.Remove(productImage);
            unitOfWork.Commit();

            return RedirectToAction(nameof(UpSert), new { id = productImage.ProductId });
        }

        #region APIs
        [HttpGet]
        public IActionResult GetAll() => Json(unitOfWork.ProductRepository.Get(includeProperties: e => e.Category));
        #endregion
    }
}
