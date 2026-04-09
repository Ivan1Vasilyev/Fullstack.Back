using Backend.Databases.Postgres;
using Backend.Repositories.Pages;
using Backend.Repositories.Providers;
using Backend.Repositories.Sites;

namespace Backend.Repositories
{
    public static class RepositoriesDi
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddSingleton<IPgConnectionFactory, PgConnectionFactory>();
            services.AddSingleton<IProvidersRepository, ProvidersRepository>();
            services.AddSingleton<ISitesRepository, SitesRepository>();
            services.AddSingleton<IPagesRepository, PagesRepository>();

            return services;
        }
    }
}
