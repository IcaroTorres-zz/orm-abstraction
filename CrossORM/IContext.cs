using CrossORM.Entities;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CrossORM
{
    public interface IContext : IDisposable
    {
        ICrossSet<TEntity> CrossSet<TEntity>() where TEntity : Entity<Guid>;
        TEntity Update<TEntity>([NotNull] TEntity entity) where TEntity : Entity<Guid>;
        IEnumerable<TEntity> UpdateRange<TEntity>([NotNull] IEnumerable<TEntity> entities) where TEntity : Entity<Guid>;
        int Save();
        Task<int> SaveAsync();
        void Rollback();
    }
}
