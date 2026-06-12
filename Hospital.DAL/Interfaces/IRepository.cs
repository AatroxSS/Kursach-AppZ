using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Hospital.DAL.Interfaces
{
    public interface IRepository<T> where T : class
    {
        IEnumerable<T> GetAll();
        T GetById(int id);

        IEnumerable<T> Find(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes);

        void Create(T item);
        void Update(T item);
        void Delete(int id);
    }
}