using CrossORM.Entities;
using CrossEF6;
using Ninject.Modules;
using System.Data.Common;

namespace CrossORM
{
    public class Module : NinjectModule
    {
        private readonly string _Environment;
        /// <summary>
        /// Constructor for a class inheriting NinjectModule, used by Ninject kernel to bind dependencies.
        /// </summary>
        /// <param name="Environment">If true, set the module environment to test.</param>
        public Module(string Environment = "release") => _Environment = Environment;

        /// <summary>
        /// Load defined custom binders for a NinjectModule
        /// </summary>
        public override void Load()
        {
            #region conditional bindings
            if (_Environment.Equals("release", System.StringComparison.OrdinalIgnoreCase))
            {
                Bind<IContext>().To<ContextEF6>().Named("EF6");
                Bind<ContextEF6>().ToSelf().Named("EF6");
            }
            else
            {
                DbConnection DonationConnection;

                if (_Environment.Equals("test", System.StringComparison.OrdinalIgnoreCase))
                {
                    DonationConnection = Effort.DbConnectionFactory.CreateTransient();
                    Bind<IContext>().ToMethod(c => new ContextEF6(DonationConnection)).Named("EF6" + _Environment);
                    Bind<ContextEF6>().ToMethod(c => new ContextEF6(DonationConnection)).Named("EF6" + _Environment);
                }
                else
                {
                    DonationConnection = Effort.DbConnectionFactory.CreatePersistent("EF6" + _Environment);
                    Bind<IContext>().ToMethod(c => new ContextEF6(DonationConnection)).Named("EF6" + _Environment);
                    Bind<ContextEF6>().ToMethod(c => new ContextEF6(DonationConnection)).Named("EF6" + _Environment);
                }
            }
            Bind<ICrossSet<Customer>>().To<EF6Set<Customer>>();
            Bind<ICrossSet<Order>>().To<EF6Set<Order>>();
            Bind<ICrossSet<Product>>().To<EF6Set<Product>>();
            Bind<ICrossSet<OrderProduct>>().To<EF6Set<OrderProduct>>();
            #endregion
        }
    }
}