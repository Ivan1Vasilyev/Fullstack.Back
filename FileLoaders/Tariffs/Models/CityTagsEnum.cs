namespace Backend.FileLoaders.Tariffs.Models
{
    public static class CityTagsEnum
    {
        public const string MOSCOW = "moscow";
        public const string MOSCOW_AREA = "moscow-area";
        public const string MOSCOW_AND_AREA = "moscow-and-area";
        public const string RUSSIA = "russia";
        public const string RUSSIA_WITHOUT_MOSCOW_AREA = "russia-without-moscow-area";
        public const string RUSSIA_WITHOUT_MOSCOW = "russia-without-moscow";

        public static string[] MOSCOW_TAGS = [CityTagsEnum.MOSCOW, CityTagsEnum.MOSCOW_AND_AREA, CityTagsEnum.RUSSIA];
        public static string[] MOSCOW_AREA_TAGS = [CityTagsEnum.RUSSIA_WITHOUT_MOSCOW, CityTagsEnum.MOSCOW_AND_AREA, CityTagsEnum.MOSCOW_AREA, CityTagsEnum.RUSSIA];
        public static string[] RUSSIA_WITHOUT_MOSCOW_AREA_TAGS = [CityTagsEnum.RUSSIA, CityTagsEnum.RUSSIA_WITHOUT_MOSCOW_AREA];
        public static string[] RUSSIA_ALL = [CityTagsEnum.RUSSIA];
    }
}
