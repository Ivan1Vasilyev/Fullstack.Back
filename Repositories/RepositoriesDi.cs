using Backend.Databases.Postgres;
using Backend.Repositories.CityTags;
using Backend.Repositories.Pages;
using Backend.Repositories.Phones;
using Backend.Repositories.Providers;
using Backend.Repositories.Sites;

namespace Backend.Repositories
{
    public static class RepositoriesDi
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddTransient<IProvidersRepository, ProvidersRepository>();
            services.AddTransient<ISitesRepository, SitesRepository>();
            services.AddTransient<IPagesRepository, PagesRepository>();
            services.AddTransient<IPhonesRepository, PhonesRepository>();
            services.AddTransient<ICityTagsRepository, CityTagsRepository>();

            return services;
        }
    }
}
