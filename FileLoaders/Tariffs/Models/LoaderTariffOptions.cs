namespace Backend.FileLoaders.Tariffs.Models
{
    public record LoaderTariffOptions(
        string Info,
        LoaderPriceInfo PriceInfo,
        LoaderInternetOptions InternetOptions,
        LoaderTvOptions TvOptions,
        LoaderMobileOptions MobileOptions,
        LoaderVideonabludenie Videonabludenie) : LoaderTariffBase(Info, PriceInfo, InternetOptions, TvOptions, MobileOptions, Videonabludenie);
}
