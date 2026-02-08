using Backend.Application.Interfaces.Services;
using Backend.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Application
{
    public static class ApplicationDi
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddSingleton<ICitiesService, CitiesService>();
            services.AddScoped<IProvidersService, ProvidersService>();
            services.AddScoped<ISitesService, SitesService>();

            return services;
        }
    }
}
