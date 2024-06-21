using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext context;

        public ICategoryRepository CategoryRepository {  get; private set; }

        public UnitOfWork(ApplicationDbContext context, ICategoryRepository categoryRepository)
        {
            this.context = context;
            //CategoryRepository = new CategoryRepository(context);
            CategoryRepository = categoryRepository;
        }

        public void Commit()
        {
            context.SaveChanges();
        }
    }
}
