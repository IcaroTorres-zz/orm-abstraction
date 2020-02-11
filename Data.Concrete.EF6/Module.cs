using Data.Abstractions;
using Data.Concrete.EF6;
using Ninject.Modules;
using Ninject.Web.Common;
using System.Data.Common;

namespace Data
{
    public class Module : NinjectModule
    {
        private readonly string _Environment;
        /// <summary>
        /// Constructor for a class inheriting NinjectModule, used by Ninject kernel to bind dependencies.
        /// </summary>
        /// <param name="Environment">If true, set the module environment to test.</param>
        public Module(string Environment = "release")
        {
            _Environment = Environment;
        }

        /// <summary>
        /// Load defined custom binders for a NinjectModule
        /// </summary>
        public override void Load()
        {
            if (_Environment.Equals("release", System.StringComparison.OrdinalIgnoreCase))
            {
                Bind<EF6Context>().ToSelf().InRequestScope().Named("EF6");
            }
            else
            {
                DbConnection DonationConnection;

                if (_Environment.Equals("test", System.StringComparison.OrdinalIgnoreCase))
                {
                    DonationConnection = Effort.DbConnectionFactory.CreateTransient();
                    Bind<EF6Context>().ToMethod(c => new EF6Context(DonationConnection))
                                      .InRequestScope()
                                      .Named("EF6" + _Environment);
                }
                else
                {
                    DonationConnection = Effort.DbConnectionFactory.CreatePersistent("EF6" + _Environment);
                    Bind<EF6Context>().ToMethod(c => new EF6Context(DonationConnection))
                                      .InRequestScope()
                                      .Named("EF6" + _Environment);
                }
            }
            Bind<IDataSource>().To<EF6Source>().InRequestScope().Named("EF6");
        }
    }
}