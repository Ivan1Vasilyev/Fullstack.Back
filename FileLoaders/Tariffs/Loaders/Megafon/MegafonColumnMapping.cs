using Backend.FileLoaders.Tariffs.ColumnMappings;

namespace Backend.FileLoaders.Tariffs.Loaders.Megafon
{
    public class MegafonColumnMapping : CommonColumnMapping
    {
        public ColumnMappingData MultiPrice { get; } = new(true, "^АП\\s+Базовая\\s+(\\d+)\\s+мбит/с,\\s+руб.$");
        public ColumnMappingData MultiOptionPrice { get; } = new(true, "^АП\\s+опции\\s+ШПД\\s+(\\d+)\\s+Мбит/с\\s*$");
        public ColumnMappingData MultiPromoPrice { get; } = new(true, "^АП\\s+в\\s+промо\\s+период\\s+(\\d+)\\s+мбит/с,\\s+руб$");
        public ColumnMappingData PromoValue { get; } = new("% скидки в промо период");
        public ColumnMappingData MegafonPromoPriceLength { get; } = new("промо период, мес.");
        public ColumnMappingData MegafonPromoPriceValue { get; } = new("% скидки в промо период");
        public ColumnMappingData MegafonChannels { get; } = new("Каналы");
        public ColumnMappingData TvPristWinkRassrochka36 { get; } = new("ТВ-приставка Wink, 36 мес.");
        public ColumnMappingData TvPristSberRassrochka36 { get; } = new("ТВ-приставка Sberbox, 36 мес.");
        public ColumnMappingData MegafonInfo { get; } = new("Опция МегаСемья");
        public ColumnMappingData MegafonInfoPrice { get; } = new("Цена за доп.сим по МегаСемье, ежемесячно");
        public ColumnMappingData MegafonInfoAddGb { get; } = new("доп.Гб при подключении опции МегаСемья, чтобы делиться");
        public ColumnMappingData MegafonMinutesAdd { get; } = new("доп.мин.");
        public ColumnMappingData MegafonGbAdd { get; } = new("доп. интернет, Гб");
    }
}
