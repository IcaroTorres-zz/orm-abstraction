using CrossORM;
using CrossORM.Entities;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CrossCore
{
    public class CoreSet<TEntity> : DbSet<TEntity>, ICrossSet<TEntity> where TEntity : Entity<Guid>
    {
        TEntity ICrossSet<TEntity>.Add([NotNull] TEntity entity) => Add(entity).Entity;
        TEntity ICrossSet<TEntity>.Attach([NotNull] TEntity entity) => Attach(entity).Entity;
        TEntity ICrossSet<TEntity>.Remove([NotNull] TEntity entity) => Remove(entity).Entity;
        IEnumerable<TEntity> ICrossSet<TEntity>.AttachRange([NotNull] IEnumerable<TEntity> entities)
        {
            AttachRange(entities);
            return entities;
        }
        IEnumerable<TEntity> ICrossSet<TEntity>.AddRange([NotNull] IEnumerable<TEntity> entities)
        {
            AddRange(entities);
            return entities;
        }
        IEnumerable<TEntity> ICrossSet<TEntity>.RemoveRange([NotNull] IEnumerable<TEntity> entities)
        {
            RemoveRange(entities);
            return entities;
        }
        IQueryable<TEntity> ICrossSet<TEntity>.AsNoTracking() => this.AsNoTracking();
        IQueryable<TEntity> ICrossSet<TEntity>.Include([NotNull] string property) => this.Include(property);
    }
}
