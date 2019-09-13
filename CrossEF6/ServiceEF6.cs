using CrossDomain;
using CrossDomain.Entities;
using JetBrains.Annotations;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;

namespace CrossEF6
{
    public class ServiceEF6 : IDisposable, IService
    {
        protected readonly DbContext Context;
        private DbTransaction ConnTransaction;
        private DbContextTransaction ContextTransaction;

        public ServiceEF6([NotNull] DbContext context) => Context = context;
        public IDisposable Begin()
        {
            if (Context.Database.Connection is SqlConnection)
                return ContextTransaction = Context.Database.BeginTransaction();
            else
                return ConnTransaction = Context.Database.Connection.BeginTransaction();
        }
        protected IQueryable<T> Set<T>() where T : Entity<Guid> => Context.Set<T>();
        protected IQueryable<T> ReadSet<T>() where T : Entity<Guid> => Context.Set<T>().AsNoTracking();
        protected bool Disposed { get; private set; } = false;
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
        public T Add<T>([NotNull] T entity) where T : Entity<Guid> => Context.Set<T>().Add(entity);

        /// <summary>
        /// Add given entities to database entity set.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        /// <returns></returns>
        public IEnumerable<T> AddRange<T>([NotNull] IEnumerable<T> entities) where T : Entity<Guid>
            => Context.Set<T>().AddRange(entities);

        /// <summary>
        /// Set given entity to be updated to database entity set.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public T Update<T>([NotNull] T entity) where T : Entity<Guid>
        {
            Context.Entry(entity).State = EntityState.Modified;
            return entity;
        }

        /// <summary>
        /// Set given entities to be updated to database entity set.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        /// <returns></returns>
        public IEnumerable<T> UpdateRange<T>([NotNull] IEnumerable<T> entities) where T : Entity<Guid>
        {
            foreach (var entity in entities)
                Context.Entry(entity).State = EntityState.Modified;

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
        public T Remove<T>([NotNull] T entity) where T : Entity<Guid> => Context.Set<T>().Remove(entity);

        /// <summary>
        /// Remove an entity from database entity set got by given key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T Remove<T>([NotNull] Guid key) where T : Entity<Guid> => Context.Set<T>().Remove(Context.Set<T>().Find(key));

        /// <summary>
        /// Remove all given entities from database set.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        /// <returns></returns>
        public IEnumerable<T> RemoveRange<T>([NotNull] IEnumerable<T> entities) where T : Entity<Guid>
            => Context.Set<T>().RemoveRange(entities);

        /// <summary>
        /// Remove all entities from database set with key in given keys.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keys"></param>
        /// <returns></returns>
        public IEnumerable<T> RemoveRange<T>([NotNull] IEnumerable<Guid> keys) where T : Entity<Guid>
        {
            var entities = Context.Set<T>()
                                  .Join(keys,
                                        entity => entity.Id,
                                        key => key,
                                        (entity, key) => entity);

            return Context.Set<T>().RemoveRange(entities);
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
                ConnTransaction?.Dispose();
                ContextTransaction?.Dispose();
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
                if (ConnTransaction != null)
                    ConnTransaction.Commit();
                else if (ContextTransaction != null)
                    ContextTransaction.Commit();
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
                if (ConnTransaction != null)
                    ConnTransaction.Rollback();
                else if (ContextTransaction != null)
                    ContextTransaction.Rollback();
                else
                    Context.ChangeTracker.Entries().ToList().ForEach(x => x.Reload());
            }
            finally { Dispose(); }
        }
        #endregion
    }
}