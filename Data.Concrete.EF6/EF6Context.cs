using Data.Entities;
using System;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Threading.Tasks;


namespace Data.Concrete.EF6
{
    public class EF6Context : DbContext
    {
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderProduct> OrderProducts { get; set; }
        public EF6Context() : base("name=ContextEF6") { }

        /// <summary>
        /// Test only constructor.
        /// </summary>
        /// <param name="connection"></param>
        public EF6Context(DbConnection connection) : base(connection, true)
        {
            Configuration.LazyLoadingEnabled = false;
            Database.SetInitializer(new DropCreateDatabaseAlways<EF6Context>());
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
