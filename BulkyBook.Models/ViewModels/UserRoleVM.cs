using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.Models.ViewModels
{
    public class UserRoleVM
    {
        public string Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        [Required]
        public string Role { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> ListOfRoles { get; set; } = null!;

        public int? Company { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> ListOfCompanies { get; set; } = null!;
    }
}
