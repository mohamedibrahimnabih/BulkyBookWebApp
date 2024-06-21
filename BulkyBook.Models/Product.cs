using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection.Metadata;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;

namespace BulkyBook.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string ISBN { get; set; } = null!;
        public string Author { get; set; } = null!;
        public string? Description { get; set; }

        [DisplayName("List Price")]
        [Range(1, 1000)]
        public double ListPrice { get; set; }

        [DisplayName("Price For 1-50")]
        [Range(1, 1000)]
        public double Price { get; set; }

        [DisplayName("Price For 50+")]
        [Range(1, 1000)]
        public double Price50 { get; set; }

        [DisplayName("Price For 100+")]
        [Range(1, 1000)]
        public double Price100 { get; set; }

        [DisplayName("Category")]
        public int CategoryId { get; set; }
        [ValidateNever]
        [ForeignKey("CategoryId")]
        public Category Category { get; set; } = null!;
    }
}
