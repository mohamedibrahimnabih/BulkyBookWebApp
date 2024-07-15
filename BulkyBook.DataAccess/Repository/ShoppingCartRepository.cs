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
    public class ShoppingCartRepository(ApplicationDbContext context) : Repository<ShoppingCart>(context), IShoppingCartRepository
    {
        public void Update(ShoppingCart shoppingCart)
        {
            var shoppingCartFromDb = context.ShoppingCarts.FirstOrDefault(e => e.Id == shoppingCart.Id);

            if (shoppingCartFromDb != null)
            {
                shoppingCartFromDb.ProductId = shoppingCart.ProductId;
                shoppingCartFromDb.ApplicationUserId = shoppingCart.ApplicationUserId;
                shoppingCartFromDb.Count = shoppingCart.Count;
            }
        }
    }
}
