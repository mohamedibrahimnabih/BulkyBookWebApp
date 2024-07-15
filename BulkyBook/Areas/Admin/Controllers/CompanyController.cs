using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBook.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = StaticData.Role_Admin)]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork unitOfWork;

        public CompanyController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public IActionResult Index() => View(unitOfWork.CompanyRepository.GetAll());

        public IActionResult UpSert(int? id)
        {
            Company? company = new Company();
            if (id != null)
            {
                company = unitOfWork.CompanyRepository.GetOne(e => e.Id == id);
            }

            return company != null ? View(company) : NotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpSert(Company company)
        {
            if (ModelState.IsValid)
            {
                if(company.Id == 0)
                {
                    unitOfWork.CompanyRepository.Add(company);

                    TempData["alert"] = "Added successfully";
                }
                else
                {
                    unitOfWork.CompanyRepository.Update(company);

                    TempData["alert"] = "Edited successfully";
                }

                unitOfWork.Commit();
                return RedirectToAction("Index");
            }

            return View(company);
        }

        public IActionResult Delete(int id)
        {
            var company = unitOfWork.CompanyRepository.GetOne(e => e.Id == id);

            if (company != null)
            {
                unitOfWork.CompanyRepository.Remove(company);
                unitOfWork.Commit();

                TempData["alert"] = "Deleted successfully";

                return RedirectToAction("Index");
            }

            return NotFound();
        }

        #region APIs
        [HttpGet]
        public IActionResult GetAll() => Json(unitOfWork.CompanyRepository.GetAll());
        #endregion
    }
}
