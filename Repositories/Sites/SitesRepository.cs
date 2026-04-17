using Backend.Databases.Postgres;
using Backend.Exceptions;
using Backend.Models.Context.Site;
using Npgsql;

namespace Backend.Repositories.Sites
{
    public class SitesRepository(IPgConnectionFactory pgConnectionFactory) : ISitesRepository
    {
        public async Task<Site?> CreateAsync(int providerId, string domainName, string? yandexCounterKey)
        {
            var exists = await ExistsByDomainAsync(domainName);
            if (exists)
                throw new ConflictException("provider", "DomainName", domainName);

            using var conn = pgConnectionFactory.GetPgConnection();
            var comm = conn.CreateCommand();
            comm.CommandText = @"
                INSERT INTO content_site (provider_id, domain_name, yandex_counter_key)
                VALUES (@ProviderId, @DomainName, @YandexCounterKey)

                RETURNING id, provider_id, domain_name, yandex_counter_key
            ";

            comm.Parameters.AddWithValue("@DomainName", domainName);
            comm.Parameters.AddWithValue("@ProviderId", providerId);
            comm.Parameters.AddWithValue("@YandexCounterKey", yandexCounterKey as object ?? DBNull.Value);

            await conn.OpenAsync();

            try
            {
                using var reader = await comm.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                    return MapToSite(reader);
            }
            catch (PostgresException ex) when (ex.SqlState == "23503")
            {
                throw new ForeignKeyException("site");
            }

            return null;
        }

        public async Task<IEnumerable<Site>> GetByProviderIdAsync(int providerId)
        {
            var result = new List<Site>();

            using (var connection = pgConnectionFactory.GetPgConnection())
            {
                var comm = connection.CreateCommand();
                comm.CommandText = @"
                  SELECT id, provider_id, domain_name, yandex_counter_key
                  FROM content_site
                  WHERE provider_id = @ProviderId
                  ORDER BY created_at 
                ";

                comm.Parameters.AddWithValue("@ProviderId", providerId);

                await connection.OpenAsync();

                using var reader = comm.ExecuteReader();
                while (reader.Read())
                {
                    result.Add(MapToSite(reader));
                }
            }

            return result;
        }

        public async Task<Site?> UpdateAsync(int id, string domainName, string? yandexCounterKey)
        {
            using var conn = pgConnectionFactory.GetPgConnection();
            var comm = conn.CreateCommand();
            comm.CommandText = @"
                UPDATE content_site
                SET domain_name = @DomainName, yandex_counter_key = @YandexCounterKey, modified_at = CURRENT_TIMESTAMP
                WHERE id = @Id
                   AND (domain_name IS DISTINCT FROM @DomainName OR yandex_counter_key IS DISTINCT FROM @YandexCounterKey)

                RETURNING id, provider_id, domain_name, yandex_counter_key
            ";

            comm.Parameters.AddWithValue("@id", id);
            comm.Parameters.AddWithValue("@DomainName", domainName as object ?? DBNull.Value);
            comm.Parameters.AddWithValue("@YandexCounterKey", yandexCounterKey as object ?? DBNull.Value);

            await conn.OpenAsync();

            try
            {
                using var reader = await comm.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                    return MapToSite(reader);
            }
            catch (PostgresException ex) when (ex.SqlState == "23505")
            {
                throw new ConflictException("site", "domain", domainName);
            }

            return null;
        }

        private static Site MapToSite(NpgsqlDataReader reader)
        {
            var id = (int)reader[0];
            var providerId = (int)reader[1];
            var domainName = reader.GetString(2);
            var yandexCounterKey = reader[3] as string;

            return new Site(id, providerId, domainName, yandexCounterKey);
        }

        private async Task<bool> ExistsByDomainAsync(string domainName)
        {
            var sqlString = "SELECT EXISTS(SELECT 1 FROM content_site WHERE domain_name = @p0)";
            return await pgConnectionFactory.ExecuteScalarAsync<bool>(sqlString, [domainName]);
        }
    }

}
