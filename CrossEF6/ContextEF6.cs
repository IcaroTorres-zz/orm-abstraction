using CrossORM;
using CrossORM.Entities;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Threading.Tasks;


namespace CrossEF6
{
    public class ContextEF6 : DbContext , IContext
    {
        public EF6Set<Customer> Customers { get; set; }
        public EF6Set<Product> Products { get; set; }
        public EF6Set<Order> Orders { get; set; }
        public EF6Set<OrderProduct> OrderProducts { get; set; }
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
        public int Save() => SaveChanges();
        public Task<int> SaveAsync() => SaveChangesAsync();
        public ICrossSet<TEntity> CrossSet<TEntity>() where TEntity : Entity<Guid>
        {
            var properties = GetType().GetProperties();
            var setProperty = properties.Single(p => p.DeclaringType == typeof(EF6Set<TEntity>));
            return (EF6Set<TEntity>)setProperty.GetValue(this);
        }
        TEntity IContext.Update<TEntity>(TEntity entity)
        {
            Entry<TEntity>(entity).State = EntityState.Modified;
            return entity;
        }
        IEnumerable<TEntity> IContext.UpdateRange<TEntity>(IEnumerable<TEntity> entities)
        {
            foreach( var entity in entities)
                Entry<TEntity>(entity).State = EntityState.Modified;

            return entities;
        }
        public void Rollback() => ChangeTracker.Entries().ToList().ForEach(x => x.Reload());
    }
}
