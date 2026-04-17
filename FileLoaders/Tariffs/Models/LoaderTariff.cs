using Backend.Utils;
using System.Text.RegularExpressions;

namespace Backend.FileLoaders.Tariffs.Models
{
    public record LoaderTariff(
        string Title,
        string Info,
        bool IsAction,
        int Priority,
        LoaderPriceInfo PriceInfo,
        LoaderInternetOptions InternetOptions,
        LoaderTvOptions TvOptions,
        LoaderMobileOptions MobileOptions,
        LoaderVideonabludenie Videonabludenie,
        LoaderProvider Provider,
        IEnumerable<LoaderCity> Cities,
        IEnumerable<LoaderTariffOptions> Options,
        IEnumerable<LoaderAddService>? AddServices = null
        )
        : LoaderTariffBase(Info, PriceInfo, InternetOptions, TvOptions, MobileOptions, Videonabludenie), ILoaderCode
    {
        string _codeCache;

        private string GetCode()
        {
            if (_codeCache == null)
            {
                unchecked
                {
                    var hash = 0;

                    if (Options != null)
                    {
                        foreach (var option in Options)
                        {
                            hash += option.GetHashCode();
                        }
                    }

                    hash += Info?.GetHashCode() ?? 0;
                    hash += Videonabludenie?.GetHashCode() ?? 0;
                    hash += Provider?.GetHashCode() ?? 0;
                    hash += InternetOptions?.GetHashCode() ?? 0;
                    hash += TvOptions?.GetHashCode() ?? 0;
                    hash += MobileOptions?.GetHashCode() ?? 0;
                    hash += PriceInfo?.GetHashCode() ?? 0;

                    if(AddServices != null)
                    {
                        foreach(var service in AddServices)
                        {
                            hash += service.GetHashCode();
                        }
                    }

                    _codeCache = $"{Provider?.Code}-{LoaderCodeGenerator.GetCode(Title)}-{InternetOptions?.Code}-{TvOptions?.Code}-{MobileOptions?.Code}-{PriceInfo?.Code}-{hash}";
                    _codeCache = Regex.Replace(_codeCache, "([-]{2,})", "-");
                }
            }

            return _codeCache;
        }

        public string Code => GetCode();
    }
}