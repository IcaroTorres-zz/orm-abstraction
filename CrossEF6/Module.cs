using CrossEF6;
using Ninject.Modules;
using Ninject.Web.Common;
using System.Data.Common;
using System.Data.Entity;

namespace CrossDomain
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
            Bind<DbContext>().To<ContextEF6>()
                .InRequestScope()
                .Named("EF6");
            Bind<IService>().To<ServiceEF6>()
                .InRequestScope()
                .Named("EF6");

            #region conditional bindings
            if (_Environment.Equals("release", System.StringComparison.OrdinalIgnoreCase))
            {

                Bind<ContextEF6>().ToSelf()
                    .InRequestScope()
                    .Named("EF6");
            }
            else
            {
                DbConnection DonationConnection;

                if (_Environment.Equals("test", System.StringComparison.OrdinalIgnoreCase))
                {
                    DonationConnection = Effort.DbConnectionFactory.CreateTransient();
                    Bind<ContextEF6>().ToMethod(c => new ContextEF6(DonationConnection))
                                      .InRequestScope()
                                      .Named("EF6" + _Environment);
                }
                else
                {
                    DonationConnection = Effort.DbConnectionFactory.CreatePersistent("EF6" + _Environment);
                    Bind<ContextEF6>().ToMethod(c => new ContextEF6(DonationConnection))
                                      .InRequestScope()
                                      .Named("EF6" + _Environment);
                }
            }
            #endregion
        }
    }
}