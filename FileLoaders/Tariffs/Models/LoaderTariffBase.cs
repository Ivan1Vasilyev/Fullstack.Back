namespace Backend.FileLoaders.Tariffs.Models
{
    public abstract record LoaderTariffBase
    (
        string Info,
        LoaderPriceInfo PriceInfo,
        LoaderInternetOptions InternetOptions,
        LoaderTvOptions TvOptions,
        LoaderMobileOptions MobileOptions,
        LoaderVideonabludenie Videonabludenie
    );
}
