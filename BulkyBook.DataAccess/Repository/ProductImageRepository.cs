using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository
{
    public class ProductImageRepository(ApplicationDbContext context) : Repository<ProductImage>(context), IProductImageRepository
    {
        public void Update(ProductImage productImage)
        {
            context.ProductImages.Update(productImage);
        }
    }
}
