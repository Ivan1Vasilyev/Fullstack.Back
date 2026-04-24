using Backend.Databases.Postgres;
using Backend.FileLoaders.Tariffs.Models;
using Backend.Models.Context.Common;
using Backend.Utils.Excel;
using System.Data;

namespace Backend.FileLoaders.Tariffs.Loaders.Tele2
{
    public class Tele2CommonLoader(IPgConnectionFactory connectionFactory, IExcelHelper excelHelper) : PgExcelFileLoader(connectionFactory, excelHelper)
    {
        public override string LoaderName => "Теле2 файл";

        public override string TargetCode => "tele2";

        private class TariffOptions
        {
            public bool BezlimitTele2 { get; set; }
            public bool BezlimitWhatsApp { get; set; }
            public bool BezlimitTelegram { get; set; }
            public bool BezlimitTamTam { get; set; }
            public bool BezlimitVk { get; set; }
            public bool BezlimitVkServices { get; set; }
            public bool BezlimitOdnoklassniki { get; set; }
            public bool BezlimitTikTok { get; set; }
            public bool BezlimitRutube { get; set; }
            public bool BezlimitTwitch { get; set; }
            public bool BezlimitDiscord { get; set; }
            public bool WinkOptium { get; set; }
        }

        Dictionary<string, TariffOptions> _tariffOptions = new(StringComparer.InvariantCultureIgnoreCase)
        {
            {
                "Мой разговор",
                new TariffOptions()
                {
                    BezlimitTele2 = true,
                }
            },
            {
                "Мой онлайн",
                new TariffOptions()
                {
                    BezlimitTele2 = true,
                    BezlimitWhatsApp = true,
                    BezlimitTelegram = true,
                    BezlimitVk = true,
                    BezlimitVkServices = true,
                    BezlimitOdnoklassniki = true,
                }
            },
            {
                "Везде онлайн",
                new TariffOptions()
                {
                    BezlimitTele2 = true,
                    BezlimitWhatsApp = true,
                    BezlimitTelegram = true,
                    BezlimitVk = true,
                    BezlimitVkServices = true,
                    BezlimitOdnoklassniki = true,
                }
            },
            {
                "Мой онлайн+",
                new TariffOptions()
                {
                    BezlimitTele2 = true,
                    BezlimitWhatsApp = true,
                    BezlimitTelegram = true,
                    BezlimitVk = true,
                    BezlimitVkServices = true,
                    BezlimitOdnoklassniki = true,
                    BezlimitTikTok = true,
                    BezlimitRutube = true
                }
            },
            {
                "Супер онлайн+",
                new TariffOptions()
                {
                    BezlimitTele2 = true,
                    BezlimitWhatsApp = true,
                    BezlimitTelegram = true,
                    BezlimitVk = true,
                    BezlimitOdnoklassniki = true,
                    BezlimitTikTok = true,
                    BezlimitRutube = true,
                    WinkOptium = true,
                }
            },
            {
                "Black",
                new TariffOptions()
                {
                    BezlimitTele2 = true,
                    BezlimitWhatsApp = true,
                    BezlimitTelegram = true,
                    BezlimitTamTam = true,
                    BezlimitVk = true,
                    BezlimitVkServices = true,
                    BezlimitOdnoklassniki = true,
                    BezlimitTikTok = true,
                    BezlimitRutube = true,
                    WinkOptium = true,
                }
            },
            {
                "Premium",
                new TariffOptions()
                {
                    BezlimitTele2 = true,
                    BezlimitWhatsApp = true,
                    BezlimitTelegram = true,
                    BezlimitTamTam = true,
                    BezlimitVk = true,
                    BezlimitVkServices = true,
                    BezlimitOdnoklassniki = true,
                    BezlimitTikTok = true,
                    WinkOptium = true,
                }
            },
            {
                "Игровой",
                new TariffOptions()
                {
                    BezlimitTwitch = true,
                    BezlimitDiscord = true,
                }
            },
            {
                "Хватит",
                new TariffOptions()
                {
                    BezlimitTele2 = true
                }
            }
        };

        protected override IEnumerable<LoaderTariff> Processing(DataSet book)
        {
            var tariffList = book.Tables["выгрузка_тарифов_с_шпд"];

            var started = false;

            var columnNames = new Dictionary<string, string>();

            var tariffs = new List<LoaderTariff>();

            var provider = new LoaderProvider("Теле2");

            foreach (DataRow row in tariffList.Rows)
            {
                if (started)
                {
                    var tarifName = row["C"] as string;
                    var city = row["A"] as string;

                    var priceMain = row["D"] as string;

                    if (string.IsNullOrWhiteSpace(tarifName) || string.IsNullOrWhiteSpace(city) || string.IsNullOrWhiteSpace(priceMain))
                    {
                        continue;
                    }

                    tarifName = tarifName.Trim();
                    city = city.Trim();
                    priceMain = priceMain.Trim();

                    //var priceAdd = row["M"] as string;
                    var min = row["E"] as string;
                    var gb = row["F"] as string;
                    var sms = row["G"] as string;
                    //var action = row["H"] as string;
                    var priority = 1;
                    //int.Parse(row["K"] as string ?? "1");

                    if (tarifName == "Black")
                    {
                        priority = 100;
                    }

                    var internetSpeed = row["I"] as string;

                    //var isAction = string.IsNullOrWhiteSpace(action);
                    //var addInfo = isAction ? null : action;
                    var addInfo = string.Empty;
                    if (tarifName == "Black" || tarifName == "Premium")
                    {
                        addInfo = "На тарифе доступна раздача трафика по Wi-Fi";
                    }
                    if (tarifName == "Хватит")
                    {
                        addInfo = "Безлимитный интернет в выходные и праздничные дни";
                    }

                    //priceAdd = priceAdd?.Trim();
                    min = min?.Trim();
                    gb = gb?.Trim();
                    sms = sms?.Trim();
                    //action = action?.Trim();
                    internetSpeed = internetSpeed?.Trim();
                    //addInfo = addInfo?.Trim();

                    var price = int.Parse(priceMain);
                    //int? promoPrice = int.Parse(priceMain);
                    //int? promoLength = (int)((new DateTime(2025, 1, 1) - DateTime.Now.Date).TotalDays / 30);

                    //if (promoLength < 1)
                    //{
                    //promoLength = null;
                    //promoPrice = null;
                    //}

                    var loaderAddServices = new List<LoaderAddService>();

                    LoaderTvOptions? tvOptions = null;
                    LoaderMobileOptions? mobileOptions = null;
                    TariffOptions? tariffOptions = null;

                    _tariffOptions.TryGetValue(tarifName, out tariffOptions);

                    if (!string.IsNullOrWhiteSpace(gb))
                    {
                        var info = tariffOptions?.BezlimitTele2 ?? false ? "Безлимит внутри сети Tele2" : null;

                        int? mobMin = null;
                        int? mobSms = null;
                        int? mobGb = null;

                        if (int.TryParse(min, out var v1))
                        {
                            mobMin = v1;
                        }

                        if (int.TryParse(sms, out var v2))
                        {
                            mobSms = v2;
                        }

                        if (int.TryParse(gb, out var v3))
                        {
                            mobGb = v3;
                        }

                        mobileOptions = new LoaderMobileOptions(mobMin, mobSms, mobGb, info);
                    }

                    if (tariffOptions != null && tariffOptions.WinkOptium)
                    {
                        //if (tariffOptions.WinkOptium)
                        //{
                        tvOptions = new LoaderTvOptions(null, null, null, 199, new LoaderDevice("pristavka", false, 5590, null, null));
                        loaderAddServices.Add(new LoaderAddService("Подписка Wink.Optimum+", null));
                        //}
                        //else
                        //{
                        //    tvOptions = new LoaderTvOptions(null, null, null, 100, null);
                        //    loaderAddServices.Add(new LoaderAddService("Подписка Wink.Optimum+", null));
                        //    loaderAddServices.Add(new LoaderAddService("Подписка More.tv", null));
                        //}
                    }

                    if (tariffOptions != null)
                    {
                        //if (tariffOptions.BezlimitViber)
                        //{
                        //    loaderAddServices.Add(new LoaderAddService("Безлимит на Viber", null));
                        //}

                        if (tariffOptions.BezlimitWhatsApp)
                        {
                            loaderAddServices.Add(new LoaderAddService("Безлимит на WhatsApp", null));
                        }

                        if (tariffOptions.BezlimitTelegram)
                        {
                            loaderAddServices.Add(new LoaderAddService("Безлимит на Telegram", null));
                        }

                        if (tariffOptions.BezlimitTamTam)
                        {
                            loaderAddServices.Add(new LoaderAddService("Безлимит на ТамТам", null));
                        }

                        if (tariffOptions.BezlimitVk)
                        {
                            loaderAddServices.Add(new LoaderAddService("Безлимит на ВКонтакте", null));
                        }

                        if (tariffOptions.BezlimitVkServices)
                        {
                            loaderAddServices.Add(new LoaderAddService("Безлимит VK Видео, VK Клипы, VK Музыка", null));
                        }

                        if (tariffOptions.BezlimitOdnoklassniki)
                        {
                            loaderAddServices.Add(new LoaderAddService("Безлимит на Одноклассники", null));
                        }

                        if (tariffOptions.BezlimitTikTok)
                        {
                            loaderAddServices.Add(new LoaderAddService("Безлимит на TikTok", null));
                        }

                        if (tariffOptions.BezlimitRutube)
                        {
                            loaderAddServices.Add(new LoaderAddService("Безлимит на Rutube", null));
                        }

                        //if (tariffOptions.BezlimitYoutube)
                        //{
                        //    loaderAddServices.Add(new LoaderAddService("Безлимит на YouTube", null));
                        //}

                        if (tariffOptions.BezlimitTwitch)
                        {
                            loaderAddServices.Add(new LoaderAddService("Безлимит на Twitch", null));
                        }

                        if (tariffOptions.BezlimitDiscord)
                        {
                            loaderAddServices.Add(new LoaderAddService("Безлимит на Discord", null));
                        }

                    }

                    List<LoaderCity> cities = [new (city, city, CityTagsEnum.RUSSIA_ALL)];

                    var tarrif = new LoaderTariff(
                        tarifName,
                        addInfo,
                        false,
                        priority,
                        new LoaderPriceInfo(price, null, null, null, null),
                        new LoaderInternetOptions(
                            int.Parse(internetSpeed),
                            new LoaderDevice("router", false, 4300, 100, 520),
                            null,
                            InternetConnectionTehnologyEnum.Unknown
                        ),
                        tvOptions,
                        mobileOptions,
                        null,
                        provider,
                        cities,
                        [],
                        loaderAddServices
                    );

                    tariffs.Add(tarrif);
                }
                else
                {
                    var city = row["A"] as string;

                    if (!string.IsNullOrWhiteSpace(city))
                    {
                        started = city == "Регион";
                    }
                }
            }


            return tariffs;
        }
    }
}
