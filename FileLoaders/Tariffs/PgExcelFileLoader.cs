using Backend.Databases.Postgres;
using Backend.FileLoaders.Tariffs.Models;
using Backend.Utils.Excel;
using ExcelDataReader;
using Newtonsoft.Json;
using System.Data;
using System.Text;

namespace Backend.FileLoaders.Tariffs
{
    public abstract class PgExcelFileLoader(IPgConnectionFactory connectionFactory, IExcelHelper excelHelper) : ITariffFileLoader
    {
        private readonly IPgConnectionFactory _connectionFactory = connectionFactory;
        private readonly IExcelHelper _excelHelper = excelHelper;

        public abstract string LoaderName { get; }
        public abstract string TargetCode { get; }

        public async Task<IEnumerable<LoaderTariff>> LoadAsync(IFormFile file, FileLoaderOptions options)
        {
            var fileStream = file.OpenReadStream();
            var dataset = new DataSet();

            var conf = new ExcelReaderConfiguration() { FallbackEncoding = Encoding.UTF8 };

            using (var reader = ExcelReaderFactory.CreateReader(fileStream, conf))
            {
                do
                {
                    if (reader.Read())
                    {
                        var dataTable = new DataTable(reader.Name);
                        dataset.Tables.Add(dataTable);

                        for (var i = 0; i < reader.FieldCount; i++)
                        {
                            var colName = _excelHelper.GetColumnNameByIndex(i);
                            dataTable.Columns.Add(new DataColumn(colName, typeof(string)));
                        }

                        do
                        {
                            var row = dataTable.NewRow();
                            var isEmpty = true;
                            for (var i = 0; i < reader.FieldCount; i++)
                            {
                                var value = reader.GetValue(i);

                                if (DBNull.Value.Equals(value))
                                {
                                    value = null;
                                }

                                var strValue = value?.ToString();

                                if (strValue != null)
                                {
                                    row[i] = strValue;

                                    isEmpty = false;
                                }
                            }

                            if (!isEmpty)
                            {
                                dataTable.Rows.Add(row);
                            }
                        }
                        while (reader.Read());
                    }
                } while (reader.NextResult());
            }

            var tariffs = Processing(dataset);

            tariffs = SquashTarrifs(tariffs);

            if (!options.OnlyView)
            {
                tariffs = await SaveTarrifsAsync(tariffs);
            }

            return tariffs;
        }

        protected abstract IEnumerable<LoaderTariff> Processing(DataSet book);

        protected virtual IEnumerable<LoaderTariff> SquashTarrifs(IEnumerable<LoaderTariff> tariffs)
        {
            var result = new Dictionary<string, LoaderTariff>();

            foreach (var tariff in tariffs)
            {
                if (result.TryGetValue(tariff.Code, out var existedTarrif))
                {
                    result[tariff.Code] = new LoaderTariff(
                        tariff.Title,
                        tariff.Info,
                        tariff.IsAction | existedTarrif.IsAction,
                        tariff.Priority,
                        tariff.PriceInfo,
                        tariff.InternetOptions,
                        tariff.TvOptions,
                        tariff.MobileOptions,
                        tariff.Videonabludenie,
                        tariff.Provider,
                        [.. tariff.Cities.Union(existedTarrif.Cities).Distinct()],
                        tariff.Options,
                        tariff.AddServices
                    );
                }
                else
                {
                    result[tariff.Code] = tariff;
                }
            }

            return [.. result.Values];
        }

        private async Task<IEnumerable<LoaderTariff>> SaveTarrifsAsync(IEnumerable<LoaderTariff> tariffs)
        {
            var result = new List<LoaderTariff>();
            foreach (var providerGroup in tariffs.GroupBy(x => x.Provider))
            {
                //var tariffsWithProxyCities = await AddProxyCitiesToTariffsAsync(providerGroup, providerGroup.Key);
                //var cities = tariffsWithProxyCities.SelectMany(x => x.Cities).Distinct();
                var cities = providerGroup.SelectMany(x => x.Cities).Distinct();

                var providerId = await GetProviderByName(providerGroup.Key.Name);
                await SaveCitiesAsync(cities, providerId);
                await SaveTarrifsAsync(providerGroup, providerId);
                result.AddRange(providerGroup);
            }

            return result;
        }

        #region proxy cities

        private async Task<IEnumerable<LoaderTariff>> AddProxyCitiesToTariffsAsync(IEnumerable<LoaderTariff> tariffs, LoaderProvider provider)
        {
            var loadingCities = tariffs.SelectMany(x => x.Cities).Distinct().ToArray();
            var proxyCities = await GetProxyCities(provider.Code, loadingCities);

            var tariffsWithProxy = new List<LoaderTariff>();
            foreach (var tariff in tariffs)
            {
                var citiesWithProxy = AddProxyToCities(tariff.Cities, proxyCities);

                var newTariff = citiesWithProxy.Count() > tariff.Cities.Count()
                    ? new LoaderTariff(
                        tariff.Title,
                        tariff.Info,
                        tariff.IsAction,
                        tariff.Priority,
                        tariff.PriceInfo,
                        tariff.InternetOptions,
                        tariff.TvOptions,
                        tariff.MobileOptions,
                        tariff.Videonabludenie,
                        tariff.Provider,
                        citiesWithProxy,
                        tariff.Options,
                        tariff.AddServices
                        )
                    : tariff;

                tariffsWithProxy.Add(newTariff);
            }

            return tariffsWithProxy;
        }

        private IEnumerable<LoaderCity> AddProxyToCities(IEnumerable<LoaderCity> cities, Dictionary<string, List<LoaderCity>> proxyCitiesDict)
        {
            var result = new List<LoaderCity>();

            foreach (var city in cities)
            {
                result.Add(city);

                if (proxyCitiesDict.TryGetValue(city.Code, out var proxyCities))
                {
                    result.AddRange(proxyCities);
                }
            }

            return result;
        }

        private async Task<Dictionary<string, List<LoaderCity>>> GetProxyCities(string providerCode, IEnumerable<LoaderCity> loadingCities)
        {
            var result = new Dictionary<string, List<LoaderCity>>();
            var existingCityCodes = new HashSet<string>(loadingCities.Select(x => x.CityCode));

            using var conn = _connectionFactory.GetPgConnection();
            var comm = conn.CreateCommand();

            comm.CommandText = @"
                SELECT c.CityCode
                FROM [dbo].[Providers_Cities] c
                JOIN [dbo].[Providers] p ON c.ProviderId = p.Id
                WHERE p.Code = @Code;

                SELECT CityId, c.CityCode, ProxyName
                FROM [dbo].[Providers_Proxy_Cities] pc
                JOIN [dbo].[Providers_Cities] c ON c.Id = pc.CityId
                JOIN [dbo].[Providers] p ON c.ProviderId = p.Id
                WHERE p.Code = @Code
            ";

            comm.Parameters.AddWithValue("@Code", providerCode);

            await conn.OpenAsync();
            using var reader = await comm.ExecuteReaderAsync();

            while (reader.Read())
            {
                var cityCode = reader.GetString(0);
                if (!string.IsNullOrEmpty(cityCode))
                {
                    existingCityCodes.Add(cityCode);
                }
            }

            if (await reader.NextResultAsync())
            {
                while (await reader.ReadAsync())
                {
                    var cityId = reader.GetInt32(0);
                    var cityCode = reader.GetString(1);
                    var proxyName = reader.GetString(2);

                    if (string.IsNullOrEmpty(cityCode) || string.IsNullOrEmpty(proxyName)) continue;

                    var sourceCity = loadingCities.FirstOrDefault(x => x.Code == cityCode);
                    if (sourceCity is null) continue;

                    var proxyCity = new LoaderCity(proxyName, sourceCity.Region, sourceCity.Tags, null, cityId);

                    if (existingCityCodes.Contains(proxyCity.CityCode)) continue;

                    if (result.TryGetValue(cityCode, out var value))
                    {
                        value.Add(proxyCity);
                    }
                    else
                    {
                        result[cityCode] = [proxyCity];
                    }
                }
            }

            return result;
        }
        #endregion

        private async Task SaveTarrifsAsync(IEnumerable<LoaderTariff> tariffs, int providerId)
        {
            using var conn = _connectionFactory.GetPgConnection();
            await conn.OpenAsync();

            using var transaction = conn.BeginTransaction();

            try
            {
                var comm = conn.CreateCommand();
                comm.Transaction = transaction;

                comm.CommandText = @$"
                    CREATE TEMP TABLE tariff_temp (
	                     code character varying(256) NOT NULL
                        ,title character varying(64) NOT NULL
                        ,info text
                        ,is_action boolean NOT NULL
                        ,priority integer NOT NULL
                        ,options text

                        ,price integer NOT NULL
                        ,promo_price integer
                        ,promo_price_length integer
                        ,price_add_info text
                        ,connection_price integer

                        ,mob_minutes integer
                        ,mob_sms integer
                        ,mob_gb integer
                        ,mob_info text

                        ,internet_speed integer
                        ,internet_technology integer
                        ,internet_wifi_router_in_complect boolean
                        ,internet_wifi_router_buy_price integer
                        ,internet_wifi_router_arenda_price integer
                        ,internet_wifi_router_rassrochka_price integer
                        ,internet_wifi_router_rassrochka24_price integer
                        ,internet_wifi_router_rassrochka36_price integer
                        ,internet_wifi_router2_buy_price integer
                        ,internet_wifi_router2_rassrochka_price integer

                        ,tv_channels integer
                        ,tv_hd_channels integer
                        ,tv_uhd_channels integer
                        ,tv_interactive_channels integer
                        ,tv_pristavka_in_complect boolean
                        ,tv_pristavka_buy_price integer
                        ,tv_pristavka_arenda_price integer
                        ,tv_pristavka_rassrochka_price integer
                        ,tv_pristavka_rassrochka24_price integer
                        ,tv_pristavka_rassrochka36_price integer
                        ,tv_pristavka2_rassrochka_price integer
                        ,tv_pristavka2_rassrochka24_price integer
                        ,tv_pristavka2_rassrochka36_price integer

                        ,video_camera_in_complect boolean
                        ,video_camera_buy_price integer
                        ,video_camera_arenda_price integer
                        ,video_camera_rassrochka_price integer
                    );

                    CREATE TEMP TABLE tariff2city_temp (
                         code character varying(256) NOT NULL
                        ,city character varying(64) NOT NULL
                        ,region character varying(64) NOT NULL
                    );

                    CREATE TEMP TABLE tariff_service_temp (
                         code character varying(256) NOT NULL
                        ,service text NOT NULL
	                    ,price integer NULL
                    );

                    CREATE INDEX ix_tariff_service ON tariff_service_temp(code);
                ";

                await comm.ExecuteNonQueryAsync();

                var copyTariffs = @$"
                    COPY tariff_temp (
                         code
                        ,title
                        ,info
                        ,is_action
                        ,priority
                        ,options

                        ,price
                        ,promo_price
                        ,promo_price_length
                        ,price_add_info
                        ,connection_price

                        ,mob_minutes
                        ,mob_sms
                        ,mob_gb
                        ,mob_info

                        ,internet_speed
                        ,internet_technology
                        ,internet_wifi_router_in_complect
                        ,internet_wifi_router_buy_price
                        ,internet_wifi_router_arenda_price
                        ,internet_wifi_router_rassrochka_price
                        ,internet_wifi_router_rassrochka24_price
                        ,internet_wifi_router_rassrochka36_price
                        ,internet_wifi_router2_buy_price
                        ,internet_wifi_router2_rassrochka_price

                        ,tv_channels
                        ,tv_hd_channels
                        ,tv_uhd_channels
                        ,tv_interactive_channels
                        ,tv_pristavka_in_complect
                        ,tv_pristavka_buy_price
                        ,tv_pristavka_arenda_price
                        ,tv_pristavka_rassrochka_price
                        ,tv_pristavka_rassrochka24_price
                        ,tv_pristavka_rassrochka36_price
                        ,tv_pristavka2_rassrochka_price
                        ,tv_pristavka2_rassrochka24_price
                        ,tv_pristavka2_rassrochka36_price

                        ,video_camera_in_complect
                        ,video_camera_buy_price
                        ,video_camera_arenda_price
                        ,video_camera_rassrochka_price
                    ) FROM STDIN (FORMAT BINARY)
                ";

                using (var writer = conn.BeginBinaryImport(copyTariffs))
                {
                    foreach (var tariff in tariffs)
                    {
                        writer.StartRow();

                        writer.Write(tariff.Code, NpgsqlTypes.NpgsqlDbType.Varchar);
                        writer.Write(tariff.Title, NpgsqlTypes.NpgsqlDbType.Varchar);
                        writer.Write(tariff.Info as object ?? DBNull.Value, NpgsqlTypes.NpgsqlDbType.Text);
                        writer.Write(tariff.IsAction, NpgsqlTypes.NpgsqlDbType.Boolean);
                        writer.Write(tariff.Priority, NpgsqlTypes.NpgsqlDbType.Integer);
                        writer.Write(JsonConvert.SerializeObject(tariff.Options), NpgsqlTypes.NpgsqlDbType.Text);

                        writer.Write(tariff.PriceInfo.Price, NpgsqlTypes.NpgsqlDbType.Integer);
                        writer.Write(tariff.PriceInfo.PromoPrice as object ?? DBNull.Value, NpgsqlTypes.NpgsqlDbType.Integer);
                        writer.Write(tariff.PriceInfo.PromoLength as object ?? DBNull.Value, NpgsqlTypes.NpgsqlDbType.Integer);
                        writer.Write(tariff.PriceInfo.AddInfo as object ?? DBNull.Value, NpgsqlTypes.NpgsqlDbType.Text);
                        writer.Write(tariff.PriceInfo.ConnectionPrice as object ?? DBNull.Value, NpgsqlTypes.NpgsqlDbType.Integer);

                        writer.Write(tariff.MobileOptions?.Minutes as object ?? DBNull.Value, NpgsqlTypes.NpgsqlDbType.Integer);
                        writer.Write(tariff.MobileOptions?.Sms as object ?? DBNull.Value, NpgsqlTypes.NpgsqlDbType.Integer);
                        writer.Write(tariff.MobileOptions?.Gb as object ?? DBNull.Value, NpgsqlTypes.NpgsqlDbType.Integer);
                        writer.Write(tariff.MobileOptions?.Info as object ?? DBNull.Value, NpgsqlTypes.NpgsqlDbType.Text);

                        writer.Write(tariff.InternetOptions.InternetSpeed, NpgsqlTypes.NpgsqlDbType.Integer);
                        writer.Write((int?)tariff.InternetOptions.Technology as object ?? DBNull.Value, NpgsqlTypes.NpgsqlDbType.Integer);

                        writer.Write(tariff.InternetOptions.WiFiRouter?.InComplect ?? false, NpgsqlTypes.NpgsqlDbType.Boolean);
                        writer.Write(tariff.InternetOptions.WiFiRouter?.BuyPrice as object ?? DBNull.Value, NpgsqlTypes.NpgsqlDbType.Integer);
                        writer.Write(tariff.InternetOptions.WiFiRouter?.ArendaPrice as object ?? DBNull.Value, NpgsqlTypes.NpgsqlDbType.Integer);
                        writer.Write(tariff.InternetOptions.WiFiRouter?.RassrochkaPrice as object ?? DBNull.Value, NpgsqlTypes.NpgsqlDbType.Integer);
                        writer.Write(tariff.InternetOptions.WiFiRouter?.Rassrochka24Price as object ?? DBNull.Value, NpgsqlTypes.NpgsqlDbType.Integer);
                        writer.Write(tariff.InternetOptions.WiFiRouter?.Rassrochka36Price as object ?? DBNull.Value, NpgsqlTypes.NpgsqlDbType.Integer);

                        writer.Write(tariff.InternetOptions.WiFiRouter2?.BuyPrice as object ?? DBNull.Value, NpgsqlTypes.NpgsqlDbType.Integer);
                        writer.Write(tariff.InternetOptions.WiFiRouter2?.RassrochkaPrice as object ?? DBNull.Value, NpgsqlTypes.NpgsqlDbType.Integer);

                        writer.Write(tariff.TvOptions?.Channels as object ?? DBNull.Value, NpgsqlTypes.NpgsqlDbType.Integer);
                        writer.Write(tariff.TvOptions?.HdChannels as object ?? DBNull.Value, NpgsqlTypes.NpgsqlDbType.Integer);
                        writer.Write(tariff.TvOptions?.UhdChannels as object ?? DBNull.Value, NpgsqlTypes.NpgsqlDbType.Integer);
                        writer.Write(tariff.TvOptions?.InteractiveChannels as object ?? DBNull.Value, NpgsqlTypes.NpgsqlDbType.Integer);

                        writer.Write(tariff.TvOptions?.TvPristavka?.InComplect ?? false, NpgsqlTypes.NpgsqlDbType.Boolean);
                        writer.Write(tariff.TvOptions?.TvPristavka?.BuyPrice as object ?? DBNull.Value, NpgsqlTypes.NpgsqlDbType.Integer);
                        writer.Write(tariff.TvOptions?.TvPristavka?.ArendaPrice as object ?? DBNull.Value, NpgsqlTypes.NpgsqlDbType.Integer);
                        writer.Write(tariff.TvOptions?.TvPristavka?.RassrochkaPrice as object ?? DBNull.Value, NpgsqlTypes.NpgsqlDbType.Integer);
                        writer.Write(tariff.TvOptions?.TvPristavka?.Rassrochka24Price as object ?? DBNull.Value, NpgsqlTypes.NpgsqlDbType.Integer);
                        writer.Write(tariff.TvOptions?.TvPristavka?.Rassrochka36Price as object ?? DBNull.Value, NpgsqlTypes.NpgsqlDbType.Integer);

                        writer.Write(tariff.TvOptions?.TvPristavka2?.RassrochkaPrice as object ?? DBNull.Value, NpgsqlTypes.NpgsqlDbType.Integer);
                        writer.Write(tariff.TvOptions?.TvPristavka2?.Rassrochka24Price as object ?? DBNull.Value, NpgsqlTypes.NpgsqlDbType.Integer);
                        writer.Write(tariff.TvOptions?.TvPristavka2?.Rassrochka36Price as object ?? DBNull.Value, NpgsqlTypes.NpgsqlDbType.Integer);

                        writer.Write(tariff.Videonabludenie?.Videocamera?.InComplect ?? false, NpgsqlTypes.NpgsqlDbType.Boolean);
                        writer.Write(tariff.Videonabludenie?.Videocamera?.BuyPrice as object ?? DBNull.Value, NpgsqlTypes.NpgsqlDbType.Integer);
                        writer.Write(tariff.Videonabludenie?.Videocamera?.ArendaPrice as object ?? DBNull.Value, NpgsqlTypes.NpgsqlDbType.Integer);
                        writer.Write(tariff.Videonabludenie?.Videocamera?.RassrochkaPrice as object ?? DBNull.Value, NpgsqlTypes.NpgsqlDbType.Integer);
                    }

                    writer.Complete();
                }

                var copyTariff2City = $@"
                    COPY tariff2city_temp (
                         code
                        ,city
	                    ,region
                    ) FROM STDIN (FORMAT BINARY)
                ";

                using (var writer = conn.BeginBinaryImport(copyTariff2City))
                {
                    foreach (var city in tariffs.SelectMany(x => x.Cities).Distinct())
                    {
                        writer.StartRow();
                        writer.Write(city.CityCode, NpgsqlTypes.NpgsqlDbType.Varchar);
                        writer.Write(city.Name, NpgsqlTypes.NpgsqlDbType.Varchar);
                        writer.Write(city.Region, NpgsqlTypes.NpgsqlDbType.Varchar);
                    }

                    writer.Complete();
                }

                var copyServices = @$"
                    COPY tariff_service_temp (
                         code
                        ,service
	                    ,price
                    ) FROM STDIN (FORMAT BINARY)
                ";

                using (var writer = conn.BeginBinaryImport(copyServices))
                {
                    foreach (var tariff in tariffs)
                    {
                        foreach (var service in tariff.AddServices ?? [])
                        {
                            writer.StartRow();
                            writer.Write(tariff.Code, NpgsqlTypes.NpgsqlDbType.Varchar);
                            writer.Write(service.Name, NpgsqlTypes.NpgsqlDbType.Text);
                            writer.Write(service.Price, NpgsqlTypes.NpgsqlDbType.Integer);
                        }
                    }

                    writer.Complete();
                }

                comm.CommandText = @"
                    UPDATE provider_tariff SET is_archive=true WHERE provider_id = @ProviderId AND target_code = @TargetCode;

                    INSERT INTO provider_tariff (
                         target_code
                        ,provider_id
                        ,code
                        ,title
                        ,info
                        ,is_action
                        ,priority
                        ,options

                        ,price
                        ,promo_price
                        ,promo_price_length
                        ,price_add_info
                        ,connection_price

                        ,mob_minutes
                        ,mob_sms
                        ,mob_gb
                        ,mob_info

                        ,internet_speed
                        ,internet_technology
                        ,internet_wifi_router_in_complect
                        ,internet_wifi_router_buy_price
                        ,internet_wifi_router_arenda_price
                        ,internet_wifi_router_rassrochka_price
                        ,internet_wifi_router_rassrochka24_price
                        ,internet_wifi_router_rassrochka36_price
                        ,internet_wifi_router2_buy_price
                        ,internet_wifi_router2_rassrochka_price

                        ,tv_channels
                        ,tv_hd_channels
                        ,tv_uhd_channels
                        ,tv_interactive_channels
                        ,tv_pristavka_in_complect
                        ,tv_pristavka_buy_price
                        ,tv_pristavka_arenda_price
                        ,tv_pristavka_rassrochka_price
                        ,tv_pristavka_rassrochka24_price
                        ,tv_pristavka_rassrochka36_price
                        ,tv_pristavka2_rassrochka_price
                        ,tv_pristavka2_rassrochka24_price
                        ,tv_pristavka2_rassrochka36_price

                        ,video_camera_in_complect
                        ,video_camera_buy_price
                        ,video_camera_arenda_price
                        ,video_camera_rassrochka_price
                    )
                    SELECT @TargetCode, @ProviderId, * FROM tariff_temp;
                    
                    INSERT INTO provider_tariff_to_city (tariff_id, city_id, provider_id)
                    SELECT DISTINCT pt.id, pc.id, @ProviderId
                    FROM tariff2city_temp tc
                    INNER JOIN provider_region pr ON pr.region_name = tc.region AND pr.provider_id = @ProviderId
                    INNER JOIN provider_city pc ON pc.city_name = tc.city AND pc.provider_id = @ProviderId
                    INNER JOIN provider_tariff pt ON pt.code = tc.code AND pt.provider_id = @ProviderId;

                    INSERT INTO provider_tariff_service(provider_tariff_id, service, price)
                    SELECT t.id, ts.service, ts.price
                    FROM tariff_service_temp ts
                    INNER JOIN provider_tariff t ON t.provider_id = @ProviderId AND t.target_code = @TargetCode AND t.code = ts.code;
                ";

                comm.Parameters.AddWithValue("@ProviderId", providerId);
                comm.Parameters.AddWithValue("@TargetCode", TargetCode);


                await comm.ExecuteNonQueryAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task<int> GetProviderByName(string name)
        {
            var result = await _connectionFactory.ExecuteScalarAsync<int>("SELECT id FROM provider WHERE name = @p0 LIMIT 1;", [name]);
            if (result == 0) throw new Exception($"provider with name {name} not found in DB");
            return result;
        }

        private async Task SaveCitiesAsync(IEnumerable<LoaderCity> cities, int providerId)
        {
            using var conn = _connectionFactory.GetPgConnection();
            await conn.OpenAsync();

            using var transaction = conn.BeginTransaction();
            try
            {
                var comm = conn.CreateCommand();

                comm.Transaction = transaction;
                comm.CommandText = @"
                    CREATE TEMP TABLE cities_temp (
                         city_name character varying(256) NOT NULL
                        ,region character varying(256) NOT NULL
	                    ,city_code character varying(256) NOT NULL
                        ,region_code character varying(256) NOT NULL
	                    ,domain_code character varying(256) NOT NULL
	                    ,source_city_id integer
	                    ,city_type character varying(64)
                    );

                    CREATE TEMP TABLE tags_temp (
                         city_name character varying(256)
                        ,tag character varying(256)
                        ,region character varying(256)
                    );
                ";

                await comm.ExecuteNonQueryAsync();

                var copyCity = @"
                    COPY cities_temp (
                         city_name
                        ,region
	                    ,city_code
                        ,region_code
	                    ,domain_code
	                    ,source_city_id
	                    ,city_type
                    ) FROM STDIN (FORMAT BINARY)
                ";

                using (var writer = conn.BeginBinaryImport(copyCity))
                {
                    foreach (var city in cities.Select(x => new { x.Name, x.Region, x.Code, x.RegionCode, x.CityCode, x.SourceCityId, x.CityType }).Distinct().OrderBy(x => x.Code.Length))
                    {
                        writer.StartRow();
                        writer.Write(city.Name, NpgsqlTypes.NpgsqlDbType.Varchar);
                        writer.Write(city.Region, NpgsqlTypes.NpgsqlDbType.Varchar);
                        writer.Write(city.Code, NpgsqlTypes.NpgsqlDbType.Varchar);
                        writer.Write(city.RegionCode, NpgsqlTypes.NpgsqlDbType.Varchar);
                        writer.Write(city.CityCode, NpgsqlTypes.NpgsqlDbType.Varchar);
                        writer.Write(city.SourceCityId, NpgsqlTypes.NpgsqlDbType.Integer);
                        writer.Write(city.CityType, NpgsqlTypes.NpgsqlDbType.Varchar);
                    }

                    writer.Complete();
                }

                var copyTags = @"
                    COPY tags_temp (
                         city_name
	                    ,tag
                        ,region
                    ) FROM STDIN (FORMAT BINARY)
                ";

                using (var writer = conn.BeginBinaryImport(copyTags))
                {
                    foreach (var tag in cities.SelectMany(x => x.Tags.Select(y => new { City = x.Name, Tag = y, x.Region })).Distinct())
                    {
                        writer.StartRow();
                        writer.Write(tag.City, NpgsqlTypes.NpgsqlDbType.Varchar);
                        writer.Write(tag.Tag, NpgsqlTypes.NpgsqlDbType.Varchar);
                        writer.Write(tag.Region, NpgsqlTypes.NpgsqlDbType.Varchar);
                    }

                    writer.Complete();
                }

                comm.CommandText = @"
                    INSERT INTO provider_region (region_name, region_code, provider_id)
                    SELECT DISTINCT
                         src.region
                        ,src.region_code
                        ,@ProviderId
                    FROM cities_temp src
                    WHERE NOT EXISTS (
                        SELECT 1 FROM provider_region tgt
                        WHERE tgt.region_name = src.region
                        AND tgt.provider_id = @ProviderId
                    );                
                    
                    INSERT INTO provider_city (city_name, provider_id, region_id, domain_code, city_code, source_city_id, city_type)
                    SELECT DISTINCT
                         src.city_name AS name
                        ,@ProviderId
                        ,r.id AS region_id
                        ,src.domain_code
                        ,src.city_code
                        ,src.source_city_id
                        ,src.city_type
                    FROM cities_temp src
                    CROSS JOIN LATERAL (
                        SELECT id FROM provider_region 
                        WHERE region_name = src.region 
                        AND provider_id = @ProviderId 
                        LIMIT 1
                    ) r
                    WHERE NOT EXISTS (
                        SELECT 1 FROM provider_city tgt 
                        WHERE tgt.city_name = src.city_name 
                        AND tgt.provider_id = @ProviderId 
                        AND tgt.region_id = r.id
                        AND (src.city_type = tgt.city_type OR src.city_type IS NULL)
                    );
                    
                    INSERT INTO provider_tag (name, provider_id)
                    SELECT DISTINCT
                        tag
                        ,@ProviderId
                    FROM tags_temp
                    WHERE NOT EXISTS (
                        SELECT 1 FROM provider_tag tgt 
                        WHERE tgt.name = tag 
                        AND tgt.provider_id = @ProviderId
                    );
                    
                    CREATE TEMP TABLE city_to_tag_temp AS
                    SELECT DISTINCT
                        c.id AS city_id,
                        rt.id AS tag_id
                    FROM tags_temp t
                    CROSS JOIN LATERAL (
                        SELECT id FROM provider_city c
                        WHERE c.city_name = t.city_name
                        AND c.provider_id = @ProviderId
                        AND EXISTS (
                            SELECT 1 FROM provider_region r 
                            WHERE r.region_name = t.region 
                            AND r.id = c.region_id
                        )
                        LIMIT 1
                    ) c
                    CROSS JOIN LATERAL (
                        SELECT id FROM provider_tag 
                        WHERE name = t.tag 
                        AND provider_id = @ProviderId
                        LIMIT 1
                    ) rt;
                    
                    INSERT INTO provider_city_to_tag (provider_id, provider_city_id, provider_tag_id)
                    SELECT 
                         @ProviderId
                        ,city_id
                        ,tag_id
                    FROM city_to_tag_temp src
                    WHERE NOT EXISTS (
                        SELECT 1 FROM provider_city_to_tag tgt 
                        WHERE tgt.provider_city_id = src.city_id 
                        AND tgt.provider_tag_id = src.tag_id
                    );
                    
                    -- Обновление DomainCode
                    UPDATE provider_city
                    SET domain_code = city_code
                    WHERE id IN (
                        SELECT DISTINCT pc.id
                        FROM provider_city pc
                        JOIN (
                            SELECT pc2.domain_code, MIN(pc2.id) AS min_id
                            FROM provider_city pc2
                            WHERE pc2.provider_id = @ProviderId
                            AND pc2.domain_code IN (
                                SELECT domain_code
                                FROM provider_city
                                WHERE provider_id = @ProviderId
                                GROUP BY domain_code
                                HAVING COUNT(*) > 1
                            )
                            GROUP BY pc2.domain_code, pc2.city_code
                        ) dc ON pc.domain_code = dc.domain_code AND pc.id != dc.min_id
                        WHERE pc.provider_id = @ProviderId
                    );
                    
                    -- Очищаем временную таблицу
                    DROP TABLE IF EXISTS city_to_tag_temp;
                ";

                comm.Parameters.AddWithValue("@ProviderId", providerId);

                await comm.ExecuteNonQueryAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
