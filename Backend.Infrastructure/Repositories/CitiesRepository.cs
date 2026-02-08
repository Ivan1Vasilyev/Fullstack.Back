using Backend.Application.Interfaces.Repositories;
using Backend.Domain.Models;
using Backend.Infrastructure.Databases;

namespace Backend.Infrastructure.Repositories
{
    public class CitiesRepository(IPgConnectionFactory pgConnectionFactory) : ICitiesRepository
    {
        public async Task<IEnumerable<City>> GetProviderCitiesAsync(int id)
        {
            var result = new List<City>();

            using (var connection = pgConnectionFactory.GetPgConnection())
            {
                var comm = connection.CreateCommand();
                comm.CommandText = @"
                  SELECT id, provider_id, city_name, domain_code
                  FROM city WHERE provider_id = @id
                ";

                comm.Parameters.AddWithValue("@Id", id);

                await connection.OpenAsync();

                using var reader = comm.ExecuteReader();
                while (reader.Read())
                {
                    var cityId = reader.GetInt32(0);
                    var providerId = reader.GetInt32(1);
                    var cityName = reader.GetString(2);
                    var domainCode = reader.GetString(3);

                    result.Add(new City(cityId, providerId, cityName, domainCode));
                }
            }

            return result;
        }
    }
}
