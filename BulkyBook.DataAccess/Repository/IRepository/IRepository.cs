using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository.IRepository
{
    public interface IRepository<T> where T : class
    {
        IEnumerable<T> Get(Expression<Func<T, bool>>? expression = null, bool tracked = false, params Expression<Func<T, object>>[] includeProperties);
        T? GetOne(Expression<Func<T, bool>> expression, bool tracked = false, params Expression<Func<T, object>>[] includeProperties);
        void Add(T entity);
        void AddRange(IEnumerable<T> entities);
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entities);
    }
}
