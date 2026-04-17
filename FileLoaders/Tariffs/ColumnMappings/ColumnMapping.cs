namespace Backend.FileLoaders.Tariffs.ColumnMappings
{
    public abstract class ColumnMapping
    {
        public abstract ColumnMappingData City { get; }
        public abstract ColumnMappingData Region { get; }

        public abstract ColumnMappingData TariffName { get; }
        public abstract ColumnMappingData Price { get; }
        public abstract ColumnMappingData PromoPrice { get; }
        public abstract ColumnMappingData PromoPriceLength { get; }

        public abstract ColumnMappingData PriceInfo { get; }
        public abstract ColumnMappingData TariffInfo { get; }
        public abstract ColumnMappingData IsAction { get; }
        public abstract ColumnMappingData ConnectionPrice { get; }

        public abstract ColumnMappingData InternetSpeed { get; }
        public abstract ColumnMappingData InternetConnectionTehnology { get; }

        public abstract ColumnMappingData WiFiInComplect { get; }
        public abstract ColumnMappingData WiFiArenda { get; }
        public abstract ColumnMappingData WiFiBuy { get; }
        public abstract ColumnMappingData WiFiRassrochka { get; }
        public abstract ColumnMappingData WiFiRassrochka24 { get; }
        public abstract ColumnMappingData WiFiRassrochka36 { get; }

        public abstract ColumnMappingData WiFi2InComplect { get; }
        public abstract ColumnMappingData WiFi2Arenda { get; }
        public abstract ColumnMappingData WiFi2Buy { get; }
        public abstract ColumnMappingData WiFi2Rassrochka { get; }
        public abstract ColumnMappingData WiFi2Rassrochka24 { get; }
        public abstract ColumnMappingData WiFi2Rassrochka36 { get; }

        public abstract ColumnMappingData TvChannels { get; }
        public abstract ColumnMappingData HdChannels { get; }
        public abstract ColumnMappingData UhdChannels { get; }
        public abstract ColumnMappingData InteractiveTv { get; }

        public abstract ColumnMappingData TvPristInComplect { get; }
        public abstract ColumnMappingData TvPristArenda { get; }
        public abstract ColumnMappingData TvPristBuy { get; }
        public abstract ColumnMappingData TvPristRassrochka { get; }
        public abstract ColumnMappingData TvPristRassrochka24 { get; }
        public abstract ColumnMappingData TvPristRassrochka36 { get; }

        public abstract ColumnMappingData TvPrist2InComplect { get; }
        public abstract ColumnMappingData TvPrist2Arenda { get; }
        public abstract ColumnMappingData TvPrist2Buy { get; }
        public abstract ColumnMappingData TvPrist2Rassrochka { get; }
        public abstract ColumnMappingData TvPrist2Rassrochka24 { get; }
        public abstract ColumnMappingData TvPrist2Rassrochka36 { get; }

        public abstract ColumnMappingData MobMin { get; }
        public abstract ColumnMappingData MobSms { get; }
        public abstract ColumnMappingData MobGb { get; }
        public abstract ColumnMappingData MobComment { get; }

        public abstract ColumnMappingData VideocameraArenda { get; }
        public abstract ColumnMappingData Priority { get; }


        public abstract ColumnMappingData IndexName { get; }
        public abstract ColumnMappingData IndexCity { get; }
    }
}
