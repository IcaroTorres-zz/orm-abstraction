using CrossORM.Entities;
using JetBrains.Annotations;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;

namespace CrossORM
{
    public class Service<TContext> : IDisposable, IService<TContext> where TContext : IContext
    {
        protected readonly TContext _context;
        public Service([NotNull] TContext context) => _context = context;
        protected ICrossSet<TEntity> DbSet<TEntity>() where TEntity : Entity<Guid> => _context.Set<TEntity>() as ICrossSet<TEntity>;
        protected bool Disposed { get; set; } = false;
        protected SafeHandle Handle { get; } = new SafeFileHandle(IntPtr.Zero, true);

        #region getters
        /// <summary>
        /// Get an entity from database entity set with given key.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="key"></param>
        /// <param name="includes"></param>
        /// <param name="isreadonly"></param>
        /// <returns></returns>
        public TEntity Get<TEntity>([NotNull] Guid key, string includes = "", bool isreadonly = false) where TEntity : Entity<Guid>
        {
            return Find<TEntity>(e => e.Id == key, includes: includes, isreadonly: isreadonly).SingleOrDefault();
        }

        /// <summary>
        /// Get all entities from database entity set.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="isreadonly"></param>
        /// <returns></returns>
        public IQueryable<TEntity> GetAll<TEntity>(bool isreadonly = false) where TEntity : Entity<Guid>
         => isreadonly ? DbSet<TEntity>().AsNoTracking() : DbSet<TEntity>();

        /// <summary>
        /// Get all entities from database entity set, including desired navigation properties.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="includes"></param>
        /// <param name="isreadonly"></param>
        /// <returns></returns>
        public IQueryable<TEntity> GetAll<TEntity>(string includes, bool isreadonly = false) where TEntity : Entity<Guid>
        => Find<TEntity>(includes: includes, isreadonly: isreadonly);

        /// <summary>
        /// Retrieve entities with optional expression predicate, ordering and property includes.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="predicate"></param>
        /// <param name="orderExpression"></param>
        /// <param name="skip"></param>
        /// <param name="top"></param>
        /// <param name="includes"></param>
        /// <param name="isreadonly"></param>
        /// <returns></returns>
        public IQueryable<TEntity> Find<TEntity>(
            Expression<Func<TEntity, bool>> predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderExpression = null,
            int? skip = null,
            int? top = null,
            string includes = "",
            bool isreadonly = false) where TEntity : Entity<Guid>
        {
            // define if is readonly and will have no change-tracking
            var query = (isreadonly ? DbSet<TEntity>().AsNoTracking()
                                    : DbSet<TEntity>());

            foreach (var property in includes.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                query = (query as ICrossSet<TEntity>).Include(property);

            query = query.Where(predicate ?? (e => true));

            orderExpression = orderExpression ?? (q => q.OrderBy(e => e.Id));

            query = top != null ? orderExpression(query).Skip(skip ?? 0).Take(top.Value)
                                : orderExpression(query).Skip(skip ?? 0);

            return query;
        }
        #endregion

        #region setters
        /// <summary>
        /// Add given entity to database entity set.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public TEntity Add<TEntity>([NotNull] TEntity entity) where TEntity : Entity<Guid>
            => DbSet<TEntity>().Add(entity);

        /// <summary>
        /// Add given entities to database entity set.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entities"></param>
        /// <returns></returns>
        public IEnumerable<TEntity> AddRange<TEntity>([NotNull] IEnumerable<TEntity> entities) where TEntity : Entity<Guid>
            => DbSet<TEntity>().AddRange(entities);

        /// <summary>
        /// Set given entity to be updated to database entity set.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public TEntity Update<TEntity>([NotNull] TEntity entity) where TEntity : Entity<Guid>
            => _context.Update(entity);

        /// <summary>
        /// Set given entities to be updated to database entity set.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entities"></param>
        /// <returns></returns>
        public IEnumerable<TEntity> UpdateRange<TEntity>([NotNull] IEnumerable<TEntity> entities) where TEntity : Entity<Guid>
        => _context.UpdateRange(entities);

        #endregion

        #region removals
        /// <summary>
        /// Remove given entity from database entity set.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public TEntity Remove<TEntity>([NotNull] TEntity entity) where TEntity : Entity<Guid>
            => DbSet<TEntity>().Remove(entity);

        /// <summary>
        /// Remove an entity from database entity set got by given key.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public TEntity Remove<TEntity>([NotNull] Guid key) where TEntity : Entity<Guid>
            => DbSet<TEntity>().Remove(DbSet<TEntity>().Find(key));

        /// <summary>
        /// Remove all given entities from database set.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entities"></param>
        /// <returns></returns>
        public IEnumerable<TEntity> RemoveRange<TEntity>([NotNull] IEnumerable<TEntity> entities) where TEntity : Entity<Guid>
            => DbSet<TEntity>().RemoveRange(entities);

        /// <summary>
        /// Remove all entities from database set with key in given keys.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="keys"></param>
        /// <returns></returns>
        public IEnumerable<TEntity> RemoveRange<TEntity>([NotNull] IEnumerable<Guid> keys) where TEntity : Entity<Guid>
        {
            var entities = DbSet<TEntity>()
                                   .Join(keys,
                                         entity => entity.Id,
                                         key => key,
                                         (entity, key) => entity);

            return DbSet<TEntity>().RemoveRange(entities);
        }
        #endregion

        #region finishers
        /// <summary>
        /// Commit changes on entities to database or fails if no related context instance found.
        /// </summary>
        public int Save() => _context.Save();

        /// <summary>
        /// Rollback changes on entities to database to avoid missmanipulation of errores,
        /// or fails if no related context instance found.
        /// </summary>
        public void Rollback() => _context.Rollback();

        // Public implementation of Dispose pattern callable by consumers.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (Disposed) return;

            if (disposing)
                Handle.Dispose();

            Disposed = true;
        }
        #endregion
    }
}