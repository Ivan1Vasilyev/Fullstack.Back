using Backend.Application.Services;
using Backend.Services.Pages;
using Backend.Services.Providers;
using Backend.Services.Sites;

namespace Backend.Services
{
    public static class ServicesDi
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddScoped<IProvidersService, ProvidersService>();
            services.AddScoped<ISitesService, SitesService>();
            services.AddSingleton<IPagesService, PagesService>();

            return services;
        }
    }

}
