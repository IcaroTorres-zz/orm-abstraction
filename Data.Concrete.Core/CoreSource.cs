using Data.Abstractions;
using Data.Entities;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Win32.SafeHandles;
using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace Data.Concrete.Core
{
    public class CoreSource : IDataSource
    {
        private readonly CoreContext Context;
        private readonly SafeHandle Handle = new SafeFileHandle(IntPtr.Zero, true);
        private IDbContextTransaction Transaction;
        private bool Disposed = false;

        public CoreSource([NotNull] CoreContext context)
        {
            Context = context;
            Customers = new CoreSet<Customer>(Context.Customers);
            Products = new CoreSet<Product>(Context.Products);
            Orders = new CoreSet<Order>(Context.Orders);
            OrderProducts = new CoreSet<OrderProduct>(Context.OrderProducts);
        }

        public IDataSet<Customer> Customers { get; }
        public IDataSet<Product> Products { get; }
        public IDataSet<Order> Orders { get; }
        public IDataSet<OrderProduct> OrderProducts { get; }

        public IDataSource BeginTransaction()
        {
            Transaction = Context.Database.BeginTransaction();

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

                if (Transaction != null)
                {
                    Transaction.Commit();
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
                Transaction?.Dispose();
                Context.Dispose();

                Transaction = null;
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

            if (Transaction != null)
            {
                Transaction.Rollback();
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