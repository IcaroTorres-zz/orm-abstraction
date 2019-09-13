using CrossDomain.Entities;
using System;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Threading.Tasks;


namespace CrossEF6
{
    public class ContextEF6 : DbContext
    {
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderProduct> OrderProducts { get; set; }
        public ContextEF6() : base("name=ContextEF6") { }

        /// <summary>
        /// Test only constructor.
        /// </summary>
        /// <param name="connection"></param>
        public ContextEF6(DbConnection connection) : base(connection, true)
        {
            Configuration.LazyLoadingEnabled = false;
            Database.SetInitializer(new DropCreateDatabaseAlways<ContextEF6>());
        }
        public override int SaveChanges()
        {
            AddAudit();
            return base.SaveChanges();
        }
        protected override void OnModelCreating(DbModelBuilder builder)
        {
            builder.Conventions.Remove<ManyToManyCascadeDeleteConvention>();
            builder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
            builder.Conventions.Remove<ForeignKeyIndexConvention>();
        }
        public override async Task<int> SaveChangesAsync()
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
    }
}
