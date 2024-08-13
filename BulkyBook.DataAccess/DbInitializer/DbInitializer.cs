using BulkyBook.DataAccess.Data;
using BulkyBook.Models;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {

        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public DbInitializer(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _context = context;
        }

        public void Initialize()
        {
            // Migrations if they are not applied
            try
            {
                if (_context.Database.GetPendingMigrations().Any())
                {
                    _context.Database.Migrate();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            // Create roles if they are not created
            if (_roleManager.Roles.IsNullOrEmpty())
            {
                _roleManager.CreateAsync(new(StaticData.Role_Admin)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new(StaticData.Role_Customer)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new(StaticData.Role_Employee)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new(StaticData.Role_Company)).GetAwaiter().GetResult();

                // If roles are not created, then we will create admin user as well
                _userManager.CreateAsync(new ApplicationUser
                {
                    UserName = "admin@eraasoft.com",
                    Email = "admin@eraasoft.com",
                    Name = "Mohamed Ibrahim",
                    PhoneNumber = "1112223333",
                    StreetAddress = "test 123 Ave",
                    State = "IL",
                    ZipCode = "23422",
                    City = "Cairo"
                }, "Admin123*").GetAwaiter().GetResult();


                ApplicationUser user = _context.ApplicationUsers.FirstOrDefault(u => u.Email == "admin@eraasoft.com");
                _userManager.AddToRoleAsync(user, StaticData.Role_Admin).GetAwaiter().GetResult();
            }
        }
    }
}
