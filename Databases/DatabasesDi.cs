using Backend.Databases.Postgres;

namespace Backend.Databases
{
    public static class DatabasesDi
    {
        public static IServiceCollection AddDatabases(this IServiceCollection services)
        {
            services.AddSingleton<IPgConnectionFactory, PgConnectionFactory>();
            
            return services;
        }
    }
}
