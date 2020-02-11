using Data.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace Data.Concrete.Core
{
    public static class IServiceCollectionExtension
    {
        public static IServiceCollection AddCoreContextLibrary(this IServiceCollection services)
        {
            // your context dependency registration
            return services
                .AddDbContext<CoreContext>(options => options.UseInMemoryDatabase("Coretest"))
                .AddScoped<IDataSource, CoreSource>();
        }
    }
}
