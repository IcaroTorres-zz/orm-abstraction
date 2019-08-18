using CrossORM.Entities;
using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CrossORM
{
    public interface ICrossSet<TEntity> 
        : IQueryable<TEntity>, IEnumerable<TEntity>, IQueryable, IEnumerable where TEntity : Entity<Guid>
    {
        TEntity Attach([NotNull] TEntity entity);
        TEntity Add([NotNull]TEntity entity);
        TEntity Find([CanBeNull] params object[] keyValues);
        TEntity Remove([NotNull] TEntity entity);
        IEnumerable<TEntity> AttachRange([NotNull]IEnumerable<TEntity> entities);
        IEnumerable<TEntity> AddRange([NotNull]IEnumerable<TEntity> entities);
        IEnumerable<TEntity> RemoveRange([NotNull] IEnumerable<TEntity> entities);
        IQueryable<TEntity> AsNoTracking();
        IQueryable<TEntity> Include(string property);
    }
}
