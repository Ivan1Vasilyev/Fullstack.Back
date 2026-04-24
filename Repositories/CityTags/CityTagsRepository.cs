using Backend.Databases.Postgres;
using Backend.Models.Context.CityTag;
using Npgsql;

namespace Backend.Repositories.CityTags
{
    public class CityTagsRepository(IPgConnectionFactory pgConnectionFactory) : ICityTagsRepository
    {
        public async Task<IEnumerable<CityTagModel>> GetByProviderId(int id)
        {
            var result = new List<CityTagModel>();

            using var conn = pgConnectionFactory.GetPgConnection();
            var comm = conn.CreateCommand();

            comm.CommandText = @"
                SELECT id, provider_id, name
                FROM provider_tag
                WHERE provider_id = @Id
            ";

            comm.Parameters.AddWithValue("@Id", id);

            await conn.OpenAsync();

            using (var reader = comm.ExecuteReader())
            {
                while (reader.Read())
                {
                    result.Add(MapToCityTag(reader));
                }
            }

            return result;
        }

        private static CityTagModel MapToCityTag(NpgsqlDataReader reader)
        {
            var id = reader.GetInt32(0);
            var providerId = reader.GetInt32(1);
            var name = reader.GetString(2);

            return new CityTagModel(id, providerId, name);
        }
    }
}
