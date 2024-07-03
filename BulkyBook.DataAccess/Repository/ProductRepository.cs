using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository
{
    internal class ProductRepository(ApplicationDbContext context) : Repository<Product>(context), IProductRepository
    {
        public void Update(Product product)
        {
            var productFromDb = context.Products.FirstOrDefault(e => e.Id == product.Id);

            if (productFromDb != null)
            {
                productFromDb.Title = product.Title;
                productFromDb.Author = product.Author;
                productFromDb.Description = product.Description;
                productFromDb.ISBN = product.ISBN;
                productFromDb.ListPrice = product.ListPrice;
                productFromDb.Price = product.Price;
                productFromDb.Price100 = product.Price100;
                productFromDb.Price50 = product.Price50;
                productFromDb.CategoryId = product.CategoryId;

                if(product.ImgURL != null)
                {
                    productFromDb.ImgURL = product.ImgURL;
                }
            }
        }
    }
}
