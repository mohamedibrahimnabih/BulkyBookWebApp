using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBook.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = ($"{StaticData.Role_Admin},{StaticData.Role_Employee}"))]
    public class UserController : Controller
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly UserManager<IdentityUser> userManager;

        public UserController(IUnitOfWork unitOfWork, UserManager<IdentityUser> userManager)
        {
            this.unitOfWork = unitOfWork;
            this.userManager = userManager;
        }

        public IActionResult Index() => View();

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
