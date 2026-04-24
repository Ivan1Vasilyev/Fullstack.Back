namespace Backend.Models.Context.Common
{
    public static class CityTagsEnum
    {
        public const string MOSCOW = "moscow";
        public const string MOSCOW_AREA = "moscow-area";
        public const string MOSCOW_AND_AREA = "moscow-and-area";
        public const string RUSSIA = "russia";
        public const string RUSSIA_WITHOUT_MOSCOW_AREA = "russia-without-moscow-area";
        public const string RUSSIA_WITHOUT_MOSCOW = "russia-without-moscow";

        public static string[] MOSCOW_TAGS = [MOSCOW, MOSCOW_AND_AREA, RUSSIA];
        public static string[] MOSCOW_AREA_TAGS = [RUSSIA_WITHOUT_MOSCOW, MOSCOW_AND_AREA, MOSCOW_AREA, RUSSIA];
        public static string[] RUSSIA_WITHOUT_MOSCOW_AREA_TAGS = [RUSSIA, RUSSIA_WITHOUT_MOSCOW_AREA];
        public static string[] RUSSIA_ALL = [RUSSIA];
    }
}
