using Backend.FileLoaders.Tariffs;
using Backend.FileLoaders.Tariffs.Loaders.Beeline;
using Backend.FileLoaders.Tariffs.Loaders.Megafon;
using Backend.FileLoaders.Tariffs.Loaders.Mts;
using Backend.FileLoaders.Tariffs.Loaders.Rinet;
using Backend.FileLoaders.Tariffs.Loaders.Rostelecom;
using Backend.FileLoaders.Tariffs.Loaders.Tele2;

namespace Backend.FileLoaders
{
    public static class FileLoadersDi
    {
        public static IServiceCollection AddFileLoaders(this IServiceCollection services)
        {
            services.AddSingleton<IFileLoaderService, FileLoaderService>();
            services.AddTransient<ITariffFileLoader, MtsMoscowAreaCommonLoader>();
            services.AddTransient<ITariffFileLoader, MtsRfCommonLoader>();
            services.AddTransient<ITariffFileLoader, RinetCommonLoader>();
            services.AddTransient<ITariffFileLoader, RostelecomCommonLoader>();
            services.AddTransient<ITariffFileLoader, Tele2CommonLoader>();
            services.AddTransient<ITariffFileLoader, BeelineCommonLoader>();
            services.AddTransient<ITariffFileLoader, BeelineWithOptionsCommonLoader>();
            services.AddTransient<ITariffFileLoader, MegafonCommonLoader>();

            return services;
        }
    }
}
