using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyBook.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = ($"{StaticData.Role_Admin},{StaticData.Role_Employee}"))]
    public class UserController : Controller
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly UserManager<IdentityUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;

        public UserController(IUnitOfWork unitOfWork, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            this.unitOfWork = unitOfWork;
            this.userManager = userManager;
            this.roleManager = roleManager;
        }

        public IActionResult Index() => View();

        public async Task<IActionResult> RoleManagement(string userId)
        {
            var user = unitOfWork.ApplicationUserRepository.GetOne(e => e.Id == userId, includeProperties: e => e.Company);

            if(user != null)
            {
                UserRoleVM userRoleVm = new()
                {
                    Id = userId,
                    Name = user.Name,
                    Email = user.Email,
                    Role = string.Join(", ", await userManager.GetRolesAsync(user)),
                    ListOfRoles = roleManager.Roles.Select(e => new SelectListItem
                    {
                        Value = e.Name,
                        Text = e.Name
                    }),
                    Company = user.Company?.Id,
                    ListOfCompanies = unitOfWork.CompanyRepository.Get().Select(e => new SelectListItem
                    {
                        Text = e.Name,
                        Value = e.Id.ToString()
                    })
                };
                return View(userRoleVm);
            }

            return NotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RoleManagement(UserRoleVM userRoleVM)
        {
            if(ModelState.IsValid)
            {
                var user = unitOfWork.ApplicationUserRepository.GetOne(e => e.Id == userRoleVM.Id, tracked: true, includeProperties: e => e.Company);

                if(user != null)
                {
                    await userManager.RemoveFromRolesAsync(user, await userManager.GetRolesAsync(user));
                    await userManager.AddToRoleAsync(user, userRoleVM.Role);

                    user.CompanyId = userRoleVM.Role == StaticData.Role_Company ? userRoleVM.Company : null;

                    unitOfWork.Commit();

                    TempData["alert"] = "Edited successfully";
                    return RedirectToAction(nameof(Index));
                }

                return NotFound();
            }
            return View(userRoleVM);
        }

        #region APIs
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = unitOfWork.ApplicationUserRepository.Get(includeProperties: e => e.Company);

            // Retrieve the roles for each user
            foreach (var user in users)
            {
                user.Role = string.Join(", ", await userManager.GetRolesAsync(user));
            }

            return Json(users);
        }

        [HttpPost]
        public async Task<IActionResult> LockUnlock([FromBody] string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            if (user.LockoutEnd != null && user.LockoutEnd > DateTime.Now)
            {
                // Unlock the user
                user.LockoutEnd = null;
                await userManager.UpdateAsync(user);
                return Json(new { success = true, message = "User unlocked successfully" });
            }
            else
            {
                // Lock the user
                user.LockoutEnd = DateTime.Now.AddYears(1000);
                await userManager.UpdateAsync(user);
                return Json(new { success = true, message = "User locked successfully" });
            }
        }

        #endregion
    }
}
