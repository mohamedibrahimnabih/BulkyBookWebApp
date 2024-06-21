using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace BulkyBook.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int DisplayOrder { get; set; }

        [ValidateNever]
        public ICollection<Product> Products { get; } = new List<Product>();
    }
}
