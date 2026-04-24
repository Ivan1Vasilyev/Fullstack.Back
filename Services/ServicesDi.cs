using Backend.Application.Services;
using Backend.Services.CityTags;
using Backend.Services.Pages;
using Backend.Services.Phones;
using Backend.Services.Providers;
using Backend.Services.Sites;

namespace Backend.Services
{
    public static class ServicesDi
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddTransient<IProvidersService, ProvidersService>();
            services.AddTransient<ISitesService, SitesService>();
            services.AddTransient<IPagesService, PagesService>();
            services.AddTransient<IPhonesService, PhonesService>();
            services.AddTransient<ICityTagsService, CityTagsService>();

            return services;
        }
    }

}
