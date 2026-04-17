using Backend.Utils.Excel;

namespace Backend.Utils
{
    public static class UtilsDi
    {
        public static IServiceCollection AddUtils(this IServiceCollection services)
        {
            services.AddSingleton<IExcelHelper, ExcelHelper>();

            return services;
        }
    }
}
