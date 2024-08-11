using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        internal readonly ApplicationDbContext context;
        private DbSet<T> dbSet;

        public Repository(ApplicationDbContext context)
        {
            this.context = context;
            dbSet = context.Set<T>();
        }

        public void Add(T entity)
        {
            dbSet.Add(entity);
        }

		public void AddRange(IEnumerable<T> entities)
		{
			dbSet.AddRange(entities);
		}

        public void Remove(T entity)
        {
            dbSet.Remove(entity);
        }

        public void RemoveRange(IEnumerable<T> entities)
        {
            dbSet.RemoveRange(entities);
        }

        public IEnumerable<T> Get(Expression<Func<T, bool>>? expression = null, bool tracked = false, params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> query = tracked ? dbSet : dbSet.AsNoTracking();

            if (expression != null)
            {
                query = query.Where(expression);
            }

            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }

            return query.ToList();
        }

        public T? GetOne(Expression<Func<T, bool>> expression, bool tracked = false, params Expression<Func<T, object>>[] includeProperties)
        {
            return Get(expression, tracked, includeProperties).FirstOrDefault();
        }
    }
}
