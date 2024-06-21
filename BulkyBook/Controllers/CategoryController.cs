using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBook.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ICategoryRepository categoryRepository;

        public CategoryController(ICategoryRepository categoryRepository)
        {
            this.categoryRepository = categoryRepository;
        }

        public IActionResult Index() => View(categoryRepository.GetAll());

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Category category)
        {
            if(ModelState.IsValid)
            {
                categoryRepository.Add(category);
                categoryRepository.Save();

                TempData["alert"] = "Added successfully";

                return RedirectToAction("Index");
            }

            return View();
        }

        // public IActionResult Edit(int id) => context.Categories.Find(id) != null ? View(context.Categories.Find(id)) : NotFound(); // Dublicate call form DB

        public IActionResult Edit(int id)
        {
            var category = categoryRepository.GetOne(e => e.Id == id);

            return category != null ? View(category) : NotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                categoryRepository.Update(category);
                categoryRepository.Save();

                TempData["alert"] = "Edited successfully";

                return RedirectToAction("Index");
            }

            return View();
        }

        public IActionResult Delete(int id)
        {
            var category = categoryRepository.GetOne(e => e.Id == id);

            if (category != null)
            {
                categoryRepository.Remove(category);
                categoryRepository.Save();

                TempData["alert"] = "Deleted successfully";

                return RedirectToAction("Index");
            }

            return NotFound();
        }
    }
}
