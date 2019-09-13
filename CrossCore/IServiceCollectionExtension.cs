using CrossDomain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CrossCore
{
    public static class IServiceCollectionExtension
    {
        public static IServiceCollection AddCoreContextLibrary(this IServiceCollection services)
        {
            // your context dependency registration
            return services
                .AddEntityFrameworkInMemoryDatabase()
                .AddDbContext<ContextCore>(options => options.UseInMemoryDatabase("Coretest"))
                .AddScoped<DbContext, ContextCore>()
                .AddScoped<ContextCore>()
                .AddScoped<IService, ServiceCore>();
        }
    }
}
