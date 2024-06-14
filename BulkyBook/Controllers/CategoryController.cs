using BulkyBook.DataAccess.Data;
using BulkyBook.Models;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBook.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext context;

        public CategoryController(ApplicationDbContext context)
        {
            this.context = context;
        }

        public IActionResult Index() => View(context.Categories.ToList());

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Category category)
        {
            if(ModelState.IsValid)
            {
                context.Categories.Add(category);
                context.SaveChanges();

                TempData["alert"] = "Added successfully";

                return RedirectToAction("Index");
            }

            return View();
        }

        // public IActionResult Edit(int id) => context.Categories.Find(id) != null ? View(context.Categories.Find(id)) : NotFound(); // Dublicate call form DB

        public IActionResult Edit(int id)
        {
            var category = context.Categories.Find(id);

            return category != null ? View(category) : NotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                context.Categories.Update(category);
                context.SaveChanges();

                TempData["alert"] = "Edited successfully";

                return RedirectToAction("Index");
            }

            return View();
        }

        public IActionResult Delete(int id)
        {
            var category = context.Categories.Find(id);

            if (category != null)
            {
                context.Categories.Remove(category);
                context.SaveChanges();

                TempData["alert"] = "Deleted successfully";

                return RedirectToAction("Index");
            }

            return NotFound();
        }
    }
}
