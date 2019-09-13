using CrossDomain.Entities;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace CrossDomain
{
    public interface IService : IDisposable
    {
        IDisposable Begin();

        #region getters
        TEntity Get<TEntity>([NotNull] Guid key, string includes = "", bool isreadonly = false) where TEntity : Entity<Guid>;
        IQueryable<TEntity> GetAll<TEntity>(bool isreadonly = false) where TEntity : Entity<Guid>;
        IQueryable<TEntity> GetAll<TEntity>(string includes, bool isreadonly = false) where TEntity : Entity<Guid>;
        IQueryable<TEntity> Find<TEntity>(Expression<Func<TEntity, bool>> predicate = null,
                                          Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderExpression = null,
                                          int? skip = null,
                                          int? top = null,
                                          string includes = "",
                                          bool isreadonly = false) where TEntity : Entity<Guid>;
        #endregion

        #region setters
        TEntity Add<TEntity>([NotNull] TEntity entity) where TEntity : Entity<Guid>;
        IEnumerable<TEntity> AddRange<TEntity>([NotNull] IEnumerable<TEntity> entities) where TEntity : Entity<Guid>;
        TEntity Update<TEntity>([NotNull] TEntity entity) where TEntity : Entity<Guid>;
        IEnumerable<TEntity> UpdateRange<TEntity>([NotNull] IEnumerable<TEntity> entities) where TEntity : Entity<Guid>;
        #endregion

        #region removals
        TEntity Remove<TEntity>([NotNull] TEntity entity) where TEntity : Entity<Guid>;
        TEntity Remove<TEntity>([NotNull] Guid key) where TEntity : Entity<Guid>;
        IEnumerable<TEntity> RemoveRange<TEntity>([NotNull] IEnumerable<TEntity> entities) where TEntity : Entity<Guid>;
        IEnumerable<TEntity> RemoveRange<TEntity>([NotNull] IEnumerable<Guid> keys) where TEntity : Entity<Guid>;
        #endregion

        #region finishers
        void Commit(bool dispose);
        void Rollback();
        #endregion
    }
}