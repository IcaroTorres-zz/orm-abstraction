using Data.Abstractions;
using Data.Entities;
using JetBrains.Annotations;
using Microsoft.Win32.SafeHandles;
using System;
using System.Data.Common;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.InteropServices;

namespace Data.Concrete.EF6
{
    public class EF6Source : IDataSource
    {
        private readonly EF6Context Context;
        private readonly SafeHandle Handle = new SafeFileHandle(IntPtr.Zero, true);
        private DbTransaction ConnectionTransaction;
        private DbContextTransaction ContextTransaction;
        private bool Disposed = false;

        public EF6Source([NotNull] EF6Context context)
        {
            Context = context;
            Customers = new EF6Set<Customer>(Context.Customers);
            Products = new EF6Set<Product>(Context.Products);
            Orders = new EF6Set<Order>(Context.Orders);
            OrderProducts = new EF6Set<OrderProduct>(Context.OrderProducts);
        }

        public IDataSet<Customer> Customers { get; }
        public IDataSet<Product> Products { get; }
        public IDataSet<Order> Orders { get; }
        public IDataSet<OrderProduct> OrderProducts { get; }

        public IDataSource BeginTransaction()
        {
            if (Context.Database.Connection is SqlConnection)
            {
                ContextTransaction = Context.Database.BeginTransaction();
            }
            else
            {
                ConnectionTransaction = Context.Database.Connection.BeginTransaction();
            }

            return this;
        }
        public void CommitState()
        {
            try
            {
                Context.SaveChanges();
            }
            catch (Exception ex)
            {
                RollbackState();
                throw ex;
            }
        }
        public void CommitTransaction()
        {
            try
            {
                CommitState();

                if (ContextTransaction != null)
                {
                    ContextTransaction.Commit();
                }
                else if (ConnectionTransaction != null)
                {
                    ConnectionTransaction.Commit();
                }
            }
            catch (Exception ex)
            {
                RollbackTransaction();
                throw ex;
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (Disposed)
            {
                return;
            }

            if (disposing)
            {
                Handle.Dispose();
                ConnectionTransaction?.Dispose();
                ContextTransaction?.Dispose();
                Context.Dispose();
                Disposed = true;
            }
        }
        public void RollbackState()
        {
            Context.ChangeTracker
                .Entries()
                .Where(e => e.State != EntityState.Added)
                .ToList()
                .ForEach(x => x.Reload());
        }
        public void RollbackTransaction()
        {
            RollbackState();

            if (ContextTransaction != null)
            {
                ContextTransaction.Rollback();
            }
            else if (ConnectionTransaction != null)
            {
                ConnectionTransaction.Rollback();
            }
        }
        public IDataSet<T> Set<T>() where T : class
        {
            return GetType()
                .GetProperties()
                .Select(pinfo => pinfo.GetValue(this))
                .OfType<IDataSet<T>>()
                .First();
        }
    }
}