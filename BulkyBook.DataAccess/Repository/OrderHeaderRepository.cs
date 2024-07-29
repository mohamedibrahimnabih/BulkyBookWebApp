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
    public class OrderHeaderRepository(ApplicationDbContext context) : Repository<OrderHeader>(context), IOrderHeaderRepository
	{
		public void Update(OrderHeader orderHeader)
		{
			context.OrderHeaders.Update(orderHeader);
		}
	}
}
