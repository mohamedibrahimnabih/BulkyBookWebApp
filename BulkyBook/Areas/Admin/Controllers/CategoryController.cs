using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBook.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork unitOfWork;

        public CategoryController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public IActionResult Index() => View(unitOfWork.CategoryRepository.GetAll());

        public IActionResult UpSert(int? id)
        {
            Category? category = new Category();
            if (id != null)
            {
                category = unitOfWork.CategoryRepository.GetOne(e => e.Id == id);
            }

            return category != null ? View(category) : NotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpSert(Category category)
        {
            if (ModelState.IsValid)
            {
                if(category.Id == 0)
                {
                    unitOfWork.CategoryRepository.Add(category);

                    TempData["alert"] = "Added successfully";
                }
                else
                {
                    unitOfWork.CategoryRepository.Update(category);

                    TempData["alert"] = "Edited successfully";
                }

                unitOfWork.Commit();
                return RedirectToAction("Index");
            }

            return View(category);
        }

        public IActionResult Delete(int id)
        {
            var category = unitOfWork.CategoryRepository.GetOne(e => e.Id == id);

            if (category != null)
            {
                unitOfWork.CategoryRepository.Remove(category);
                unitOfWork.Commit();

                TempData["alert"] = "Deleted successfully";

                return RedirectToAction("Index");
            }

            return NotFound();
        }

        #region APIs
        [HttpGet]
        public IActionResult GetAll() => Json(unitOfWork.CategoryRepository.GetAll());
        #endregion
    }
}
