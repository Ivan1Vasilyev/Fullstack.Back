using Backend.Databases.Postgres;
using Backend.FileLoaders.Tariffs.Models;
using Backend.Models.Context.Common;
using Backend.Utils.Excel;

namespace Backend.FileLoaders.Tariffs.Loaders.Rinet
{
    public class RinetCommonLoader(IPgConnectionFactory connectionFactory, IExcelHelper excelHelper) : AutoTarrifFileLoader(connectionFactory, excelHelper)
    {
        public override string LoaderName => "Ринет (наш файл)";
        public override string TargetCode => "rinet";
        protected override LoaderProvider Provider => new LoaderProvider(ProvidersEnum.RINET);
        protected override string DefaultCity => "Москва";
        protected override string DefaultRegion => "Москва и область";
        protected override string[] DefaultTags => CityTagsEnum.MOSCOW_TAGS;
    }
}
