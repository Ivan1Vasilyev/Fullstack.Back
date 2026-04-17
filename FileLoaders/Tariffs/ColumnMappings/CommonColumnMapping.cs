namespace Backend.FileLoaders.Tariffs.ColumnMappings
{
    public class CommonColumnMapping : ColumnMapping
    {
        public override ColumnMappingData ConnectionPrice { get; } = new(
            "Подключение", // МТС РФ
            "Стоимость подключения", // Билайн
            "Подключение , руб", // МТС РФ
            "стоимость платного подключения" // Мегафон
            );
        public override ColumnMappingData City { get; } = new("Город, индекс", "Город", "Индекс");
        public override ColumnMappingData Region { get; } = new("Регион", "Область");
        public override ColumnMappingData Priority { get; } = new(
            "Приоритет",
            "Кол-во доступных МегаСил на выбор" // Мегафон
            );
        public override ColumnMappingData TariffName { get; } = new("Название тарифа", "Тарифный план (наименование)", "Название тарифа");
        public override ColumnMappingData PriceInfo { get; } = new("Доп. информация по цене");
        public override ColumnMappingData TariffInfo { get; } = new("Доп. информация по тарифу", "Преимущества", "Описание");
        public override ColumnMappingData IsAction { get; } = new("Признак акции");
        public override ColumnMappingData Price { get; } = new("Цена", "Базовая АП");
        public override ColumnMappingData PromoPrice { get; } = new("Цена со скидкой", "АП в промо");
        public override ColumnMappingData PromoPriceLength { get; } = new("Длительность скидки в мес", "Промо-период, мес");
        public override ColumnMappingData MobComment { get; } = new("Комментарий к моб. связи");
        public override ColumnMappingData MobGb { get; } = new("Гб", "ГБ", "Мобильная связь, ГБ", "Моб. интернет, Гб");
        public override ColumnMappingData MobMin { get; } = new("Мин", "Мобильная связь, мин", "Минуты");
        public override ColumnMappingData MobSms { get; } = new("Смс", "СМС", "Мобильная связь, sms");
        public override ColumnMappingData InternetConnectionTehnology { get; } = new("Технология ШПД");
        public override ColumnMappingData InternetSpeed { get; } = new("Скорость мбит/сек", "Базовая скорость ШПД, мбит/с", "Скорость Мбит/сек");
        public override ColumnMappingData WiFiInComplect { get; } = new("WiFi-роутер в комплекте");
        public override ColumnMappingData WiFiArenda { get; } = new(
            "WiFi-роутер в аренду",
            "ШПД Абонентское оборудование стоимость аренды (руб/мес)", // МТС РФ
            "Роутер аренда" // Билайн
            );

        public override ColumnMappingData WiFiBuy { get; } = new(
            "WiFi-роутер покупка",
            "ШПД Выкуп роутера в собственность, руб", // МТС РФ
            "Роутер покупка" // Билайн
            );
        public override ColumnMappingData WiFiRassrochka { get; } = new(
            "WiFi-роутер в рассрочку", // МТС МО
            "Рассрочка wifi роутеров на 12 мес Ежемесячный платеж, руб", // МТС РФ
            "Рассрочка wifi оборудования (12мес/36 мес)", // МТС РФ
            "Роутер рассрочка 12 месяцев"  // Билайн
            );
        public override ColumnMappingData TvChannels { get; } = new(
            "ТВ Каналов", // МТС МО
            "Кол-во ТВ-каналов", // МТС РФ
            "Количество каналов", // Билайн
            "Количество ТВ-каналов" // Билайн
            );
        public override ColumnMappingData HdChannels { get; } = new(
            "HD каналов", // МТС МО
            "Кол-во HD-каналов", // МТС РФ
            "Количество HD каналов" // Билайн
            );
        public override ColumnMappingData UhdChannels { get; } = new(
            "UHD каналов", // МТС МО
            "Кол-во UHD-каналов", // МТС РФ
            "UHD каналов" // Билайн
            );
        public override ColumnMappingData InteractiveTv { get; } = new("Интерактивное ТВ каналов");
        public override ColumnMappingData TvPristArenda { get; } = new(
            "ТВ-приставка в аренду", // МТС МО
            "ТВ-декодеры: ТВ-модуль CAM , цена аренды, руб ",  // МТС РФ
            "ТВ-декодеры: HD-приставка , цена аренды, руб."  // МТС РФ
            );
        public override ColumnMappingData TvPristBuy { get; } = new(
            "ТВ-приставка покупка", // МТС МО
            "ТВ-декодеры Выкуп HD-приставки в собственность, руб." // МТС РФ
            );
        public override ColumnMappingData TvPristInComplect { get; } = new("ТВ-приставкав комплекте", "ТВ-приставка в комплекте");
        public override ColumnMappingData TvPristRassrochka { get; } = new(
            "ТВ-приставка рассрочка 12 месяцев", // МТС МО, Билайн
            "Рассрочка ТВ оборудования на 12 мес Ежемесячный платеж, руб" // МТС РФ
            );
        public override ColumnMappingData TvPristRassrochka24 { get; } = new("ТВ-приставка рассрочка 24 месяца"); // Билайн
        public override ColumnMappingData TvPristRassrochka36 { get; } = new(
            "Рассрочка ТВ оборудования на 36 мес Ежемесячный платеж, руб", // МТС РФ
            "ТВ-приставка, 36 мес." // Мегафон
            );

        public override ColumnMappingData VideocameraArenda { get; } = new("Аренда видеокамеры");
        public override ColumnMappingData IndexCity { get; } = new("Город");
        public override ColumnMappingData IndexName { get; } = new("Имя индекса");

        public override ColumnMappingData WiFiRassrochka24 => new("Роутер рассрочка 24 месяца"); // Билайн
        public override ColumnMappingData WiFiRassrochka36 => new(
            "Рассрочка wifi роутеров на 36 мес Ежемесячный платеж, руб", // МТС РФ
            "Рассрочка роутера 500 мбит/с на 36 мес.", // Мегафон
            "Роутер рассрочка 36 месяцев"); // Билайн

        public override ColumnMappingData WiFi2InComplect => new("НЕТ...");

        public override ColumnMappingData WiFi2Arenda => new("НЕТ...");

        public override ColumnMappingData WiFi2Buy => new("ONT-роутер покупка"); // МТС МО

        public override ColumnMappingData WiFi2Rassrochka => new("ONT-роутер рассрочка 12 месяцев"); // МТС МО

        public override ColumnMappingData WiFi2Rassrochka24 => new("НЕТ...");

        public override ColumnMappingData WiFi2Rassrochka36 => new("НЕТ...");

        public override ColumnMappingData TvPrist2InComplect => new("НЕТ...");

        public override ColumnMappingData TvPrist2Arenda => new("НЕТ...");

        public override ColumnMappingData TvPrist2Buy => new("Sberbox покупка"); // Билайн

        public override ColumnMappingData TvPrist2Rassrochka => new("Sberbox рассрочка 12 месяцев"); // Билайн

        public override ColumnMappingData TvPrist2Rassrochka24 => new("Sberbox рассрочка 24 месяца"); // Билайн

        public override ColumnMappingData TvPrist2Rassrochka36 => new("НЕТ...");
    }
}
