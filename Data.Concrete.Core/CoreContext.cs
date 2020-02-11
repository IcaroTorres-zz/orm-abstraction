using Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Data.Concrete.Core
{
    public class CoreContext : DbContext
    {
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderProduct> OrderProducts { get; set; }
        public CoreContext() { }
        public CoreContext(DbContextOptions<CoreContext> options) : base(options) { }
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
            var entities = ChangeTracker.Entries().Where(x => x.Entity is Entity && (x.State == EntityState.Added || x.State == EntityState.Modified));
            foreach (var entity in entities)
            {
                if (entity.State == EntityState.Added)
                {
                    ((Entity)entity.Entity).CreatedDate = DateTime.UtcNow;
                }

                ((Entity)entity.Entity).ModifiedDate = DateTime.UtcNow;
            }
        }
    }
}
