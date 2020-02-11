using Data.Abstractions;
using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace Data.Concrete.EF6
{
    public class EF6Set<T> : IDataSet<T> where T : class
    {
        private readonly DbSet<T> _dbset;
        private readonly IQueryable<T> _dbsetAsQueryable;
        private readonly IEnumerable<T> _dbsetAsEnumerable;

        public EF6Set(DbSet<T> dbset)
        {
            _dbset = dbset;
            _dbsetAsQueryable = _dbset as IQueryable<T>;
            _dbsetAsEnumerable = _dbset as IEnumerable<T>;
        }

        public T Add([NotNull] T entity)
        {
            return _dbset.Add(entity);
        }

        public IEnumerable<T> AddRange([NotNull] params T[] entities)
        {
            _dbset.AddRange(entities);

            return entities;
        }
        public T Find([NotNull] params object[] keys)
        {
            return _dbset.Find(keys);
        }

        public T Remove([NotNull] T entity)
        {
            return _dbset.Remove(entity);
        }

        public T Remove([NotNull] params object[] keys)
        {
            return _dbset.Remove(_dbset.Find(keys));
        }

        public IEnumerable<T> RemoveRange([NotNull] params T[] entities)
        {
            _dbset.RemoveRange(entities);

            return entities;
        }

        public Type ElementType => _dbsetAsQueryable.ElementType;
        public Expression Expression => _dbsetAsQueryable.Expression;
        public IQueryProvider Provider => _dbsetAsQueryable.Provider;
        public IEnumerator<T> GetEnumerator()
        {
            return _dbsetAsEnumerable.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _dbsetAsEnumerable.GetEnumerator();
        }
    }
}
