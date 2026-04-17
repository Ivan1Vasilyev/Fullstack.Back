using Backend.FileLoaders.Tariffs.ColumnMappings;

namespace Backend.FileLoaders.Tariffs.Loaders.Beeline
{
    public class BeelineColumnMapping : CommonColumnMapping
    {
        public ColumnMappingData MultiPromoPrice { get; } = new(true, "^Цена\\s+со\\s+скидкой\\s+(\\d+)\\s+Мбит$");

        public ColumnMappingData MultiPrice { get; } = new(true, "^Стоимость\\s+(\\d+)\\s+Мбит$");
        public ColumnMappingData MultiPromoPriceLength { get; } = new(true, "^Длительность\\s+скидки\\s+(\\d+)\\s+Мбит$");
        public ColumnMappingData MinutesAdd { get; } = new("Бонусные минуты");
        public ColumnMappingData GbAdd { get; } = new("Бонусные ГБ");
        public ColumnMappingData NewSimBuyPrice { get; } = new("Цена за новую SIM");

    }
}
