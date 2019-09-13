using CrossDomain;
using CrossDomain.Entities;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;

namespace CrossCore
{
    public class ServiceCore : IDisposable, IService
    {
        protected readonly DbContext Context;
        private IDisposable Transaction;

        public IDisposable Begin() => Transaction = Context.Database.BeginTransaction();
        public ServiceCore([NotNull] DbContext context) => Context = context;
        protected IQueryable<T> Set<T>() where T : Entity<Guid> => Context.Set<T>();
        protected IQueryable<T> ReadSet<T>() where T : Entity<Guid> => Context.Set<T>().AsNoTracking();
        protected bool Disposed { get; set; } = false;
        protected SafeHandle Handle { get; } = new SafeFileHandle(IntPtr.Zero, true);

        #region getters
        /// <summary>
        /// Get an entity from database entity set with given key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="includes"></param>
        /// <param name="isreadonly"></param>
        /// <returns></returns>
        public T Get<T>([NotNull] Guid key, string includes = "", bool isreadonly = false) where T : Entity<Guid>
        {
            return Find<T>(e => e.Id == key, includes: includes, isreadonly: isreadonly).SingleOrDefault();
        }

        /// <summary>
        /// Get all entities from database entity set.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="isreadonly"></param>
        /// <returns></returns>
        public IQueryable<T> GetAll<T>(bool isreadonly = false) where T : Entity<Guid> => isreadonly ? ReadSet<T>() : Set<T>();

        /// <summary>
        /// Get all entities from database entity set, including desired navigation properties.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="includes"></param>
        /// <param name="isreadonly"></param>
        /// <returns></returns>
        public IQueryable<T> GetAll<T>(string includes, bool isreadonly = false) where T : Entity<Guid>
        => Find<T>(includes: includes, isreadonly: isreadonly);

        /// <summary>
        /// Retrieve entities with optional expression predicate, ordering and property includes.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicate"></param>
        /// <param name="orderExpression"></param>
        /// <param name="skip"></param>
        /// <param name="top"></param>
        /// <param name="includes"></param>
        /// <param name="isreadonly"></param>
        /// <returns></returns>
        public IQueryable<T> Find<T>(
            Expression<Func<T, bool>> predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderExpression = null,
            int? skip = null,
            int? top = null,
            string includes = "",
            bool isreadonly = false) where T : Entity<Guid>
        {
            // define if is readonly and will have no change-tracking
            var query = (isreadonly ? ReadSet<T>() : Set<T>());

            foreach (var property in includes.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                query = query.Include(property);

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
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public T Add<T>([NotNull] T entity) where T : Entity<Guid> => Context.Set<T>().Add(entity).Entity;

        /// <summary>
        /// Add given entities to database entity set.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        /// <returns></returns>
        public IEnumerable<T> AddRange<T>([NotNull] IEnumerable<T> entities) where T : Entity<Guid>
        {
            Context.Set<T>().AddRange(entities);
            return entities;
        }

        /// <summary>
        /// Set given entity to be updated to database entity set.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public T Update<T>([NotNull] T entity) where T : Entity<Guid> => Context.Update(entity).Entity;

        /// <summary>
        /// Set given entities to be updated to database entity set.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        /// <returns></returns>
        public IEnumerable<T> UpdateRange<T>([NotNull] IEnumerable<T> entities) where T : Entity<Guid>
        {
            Context.Set<T>().UpdateRange(entities);
            return entities;
        }

        #endregion

        #region removals
        /// <summary>
        /// Remove given entity from database entity set.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public T Remove<T>([NotNull] T entity) where T : Entity<Guid> => Context.Set<T>().Remove(entity).Entity;

        /// <summary>
        /// Remove an entity from database entity set got by given key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T Remove<T>([NotNull] Guid key) where T : Entity<Guid> => Context.Set<T>().Remove(Context.Set<T>().Find(key)).Entity;

        /// <summary>
        /// Remove all given entities from database set.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        /// <returns></returns>
        public IEnumerable<T> RemoveRange<T>([NotNull] IEnumerable<T> entities) where T : Entity<Guid>
        {
            Context.Set<T>().RemoveRange(entities);
            return entities;
        }

        /// <summary>
        /// Remove all entities from database set with key in given keys.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keys"></param>
        /// <returns></returns>
        public IEnumerable<T> RemoveRange<T>([NotNull] IEnumerable<Guid> keys) where T : Entity<Guid>
        {
            var entities = Set<T>()
                                   .Join(keys,
                                         entity => entity.Id,
                                         key => key,
                                         (entity, key) => entity);

            Context.Set<T>().RemoveRange(entities);
            return entities;
        }
        #endregion

        #region finishers
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        /// <summary>
        /// Dispose all unmanaged objects and the opened context
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (Disposed) return;

            if (disposing)
            {
                Handle.Dispose();
                Transaction?.Dispose();
                Context.Dispose();

                Disposed = true;
            }
        }

        /// <summary>
        /// Try committing all changes in transaction and perform Rollback if fail
        /// </summary>
        public void Commit(bool dispose)
        {
            try
            {
                // commit transaction if there is one active
                if (Transaction != null)
                    Transaction.Commit();
                else
                    Context.SaveChanges();
            }
            catch (Exception dbex)
            {
                // rollback if there was an exception
                Rollback();
                throw dbex;
            }
            finally { Dispose(dispose); }
        }

        /// <summary>
        /// Discard all unsaved changes, dispatched when Commit fails and used when some part of a transaction fails
        /// </summary>
        public void Rollback()
        {
            try
            {
                if (Transaction != null)
                    Transaction.Rollback();
                else
                    Context.ChangeTracker.Entries().ToList().ForEach(x => x.Reload());
            }
            finally { Dispose(); }
        }
        #endregion
    }
}