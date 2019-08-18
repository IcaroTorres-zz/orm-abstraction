using CrossORM;
using CrossORM.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CrossCore
{
    public class ContextCore : DbContext, IContext
    {
        public CoreSet<Customer> Customers { get; set; }
        public CoreSet<Product> Products { get; set; }
        public CoreSet<Order> Orders { get; set; }
        public CoreSet<OrderProduct> OrderProducts { get; set; }
        public ContextCore() { }
        public ContextCore(DbContextOptions<ContextCore> options) : base(options) { }
        public override int SaveChanges()
        {
            AddAudit();
            return base.SaveChanges();
        }
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            AddAudit();
            return await base.SaveChangesAsync();
        }
        private void AddAudit()
        {
            var entities = ChangeTracker.Entries().Where(x => x.Entity is Entity<int> && (x.State == EntityState.Added || x.State == EntityState.Modified));
            foreach (var entity in entities)
            {
                if (entity.State == EntityState.Added)
                {
                    ((Entity<int>)entity.Entity).CreatedDate = DateTime.UtcNow;
                }

                ((Entity<int>)entity.Entity).ModifiedDate = DateTime.UtcNow;
            }
        }
        public int Save() => SaveChanges();
        public Task<int> SaveAsync() => SaveChangesAsync();
        public ICrossSet<TEntity> CrossSet<TEntity>() where TEntity : Entity<Guid>
        {
            var properties = GetType().GetProperties();
            var setProperty = properties.Single(p => p.PropertyType == typeof(CoreSet<TEntity>));
            return (ICrossSet<TEntity>)setProperty.GetValue(this);
        }
        TEntity IContext.Update<TEntity>(TEntity entity) => Update<TEntity>(entity).Entity;
        IEnumerable<TEntity> IContext.UpdateRange<TEntity>(IEnumerable<TEntity> entities)
        {
            UpdateRange(entities);
            return entities;
        }
        public void Rollback() => ChangeTracker.Entries().ToList().ForEach(x => x.Reload());
    }
}
