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
            context.Products.Update(product);
        }
    }
}
