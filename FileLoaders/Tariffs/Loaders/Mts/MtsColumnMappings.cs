using Backend.FileLoaders.Tariffs.ColumnMappings;

namespace Backend.FileLoaders.Tariffs.Loaders.Mts
{
    public class MtsColumnMapping : CommonColumnMapping
    {
        public ColumnMappingData MultiPrice { get; } = new(true, "^Стоимость\\s+(\\d+)\\s+Мбит/сек$", "Опция\\s+скорост[ьи]\\s+(\\d+)\\s+[мг]бит/с");
        public ColumnMappingData MultiOptionPrice { get; } = new(true, "Цена\\s+за\\s+Опцию\\s+скорост[ьи]\\s+(\\d+)\\s+[мг]бит/с");

        public ColumnMappingData MultiPromoPrice { get; } = new(true, "^Цена\\s+со\\s+скидкой\\s+(\\d+)\\s+Мбит/сек$");
        public ColumnMappingData MultiPromoPriceLength { get; } = new(true, "^Длительность\\s+скидки\\s+(\\d+)\\s+Мбит/сек$");
        public ColumnMappingData TvHdPristArenda { get; } = new("ТВ-декодеры: HD-приставка , цена аренды, руб.");
        public ColumnMappingData TvIptvPristArenda { get; } = new("ТВ-декодеры: IPTV-приставка , цена аренды, руб");
        public ColumnMappingData TvHdPristBuyPrice { get; } = new("ТВ-декодеры Выкуп HD-приставки в собственность, руб ");
        public ColumnMappingData RussiaComments { get; } = new("Комментарии");
        public ColumnMappingData RussiaNotes { get; } = new("Пометки");
        public ColumnMappingData MonthForFree { get; } = new("Месяц в подарок");
        public ColumnMappingData GroupForFamiliars { get; } = new("\"Группа для близких\" Стоимость за каждого акцептора, руб", "Группа \"Для близких\"");
    }
}
