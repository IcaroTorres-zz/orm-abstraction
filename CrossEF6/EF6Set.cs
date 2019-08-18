using CrossORM;
using CrossORM.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace CrossEF6
{
    public class EF6Set<TEntity> : DbSet<TEntity>, ICrossSet<TEntity> where TEntity : Entity<Guid>
    {
        TEntity ICrossSet<TEntity>.Add(TEntity entity) => Add(entity);
        TEntity ICrossSet<TEntity>.Attach(TEntity entity) => Attach(entity);
        TEntity ICrossSet<TEntity>.Remove(TEntity entity) => Remove(entity);
        IEnumerable<TEntity> ICrossSet<TEntity>.AttachRange(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities)
                Attach(entity);

            return entities;
        }
        IEnumerable<TEntity> ICrossSet<TEntity>.AddRange(IEnumerable<TEntity> entities) => AddRange(entities);
        IEnumerable<TEntity> ICrossSet<TEntity>.RemoveRange(IEnumerable<TEntity> entities) => RemoveRange(entities);
        IQueryable<TEntity> ICrossSet<TEntity>.AsNoTracking() => AsNoTracking();
        IQueryable<TEntity> ICrossSet<TEntity>.Include(string property) => Include(property);
    }
}
