using Backend.Application.Interfaces.Repositories;
using Backend.Infrastructure.Databases;
using Backend.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Infrastructure
{
    public static class InfrastructureDi
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddSingleton<IPgConnectionFactory, PgConnectionFactory>();
            services.AddSingleton<ICitiesRepository, CitiesRepository>();
            services.AddSingleton<IProvidersRepository, ProviderRepository>();
            services.AddSingleton<ISitesRepository, SitesRepository>();

            return services;
        }
    }
}
