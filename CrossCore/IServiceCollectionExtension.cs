using CrossORM;
using CrossORM.Entities;
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
                .AddScoped<IContext, ContextCore>()
                .AddScoped<ICrossSet<Customer>, CoreSet<Customer>>()
                .AddScoped<ICrossSet<Order>, CoreSet<Order>>()
                .AddScoped<ICrossSet<Product>, CoreSet<Product>>()
                .AddScoped<ICrossSet<OrderProduct>, CoreSet<OrderProduct>>()
                .AddScoped<IService<ContextCore>, Service<ContextCore>>();
        }
    }
}
