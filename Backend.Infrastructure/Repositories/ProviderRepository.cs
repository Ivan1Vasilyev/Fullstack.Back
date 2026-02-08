using Backend.Application.Interfaces.Repositories;
using Backend.Domain.Common.Exceptions;
using Backend.Domain.Models;
using Backend.Infrastructure.Databases;
using Npgsql;

namespace Backend.Infrastructure.Repositories
{
    internal class ProviderRepository(IPgConnectionFactory pgConnectionFactory) : IProvidersRepository
    {
        public async Task<IEnumerable<Provider>> GetAllAsync()
        {
            var result = new List<Provider>();

            using (var connection = pgConnectionFactory.GetPgConnection())
            {
                var comm = connection.CreateCommand();
                comm.CommandText = @"
                  SELECT id, name, code
                  FROM provider
                ";

                await connection.OpenAsync();

                using var reader = comm.ExecuteReader();
                while (await reader.ReadAsync())
                {
                    result.Add(MapToProvider(reader));
                }
            }

            return result;
        }

        public async Task<Provider> GetByIdAsync(int id)
        {
            using var connection = pgConnectionFactory.GetPgConnection();
            var comm = connection.CreateCommand();
            comm.CommandText = @"
                SELECT id, name, code
                FROM provider
                WHERE id = @id
            ";

            comm.Parameters.AddWithValue("@id", id);

            await connection.OpenAsync();

            using var reader = await comm.ExecuteReaderAsync();
            if (await reader.ReadAsync())
                return MapToProvider(reader);

            throw new EntityNotFoundException("provider", id);
        }

        public async Task<Provider?> UpdateAsync(int id, string newName, string newCode)
        {
            using var conn = pgConnectionFactory.GetPgConnection();
            var comm = conn.CreateCommand();
            comm.CommandText = @"
                UPDATE provider
                SET name = @Name, code = @Code
                WHERE id = @Id
                   AND (name IS DISTINCT FROM @Name OR code IS DISTINCT FROM @Code)

                RETURNING id, name, code
            ";

            comm.Parameters.AddWithValue("@Name", newName);
            comm.Parameters.AddWithValue("@Code", newCode);
            comm.Parameters.AddWithValue("@id", id);

            await conn.OpenAsync();

            try
            {
                using var reader = await comm.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                    return MapToProvider(reader);
            }
            catch (PostgresException ex) when (ex.SqlState == "23505")
            {
                throw new UniqueViolationException("code", newCode, "provider");
            }

            return null;
        }

        public async Task<Provider?> CreateAsync(string name, string code)
        {
            var exists = await ExistsByCodeAsync(code);
            if (exists)
                throw new UniqueViolationException("code", code, "provider");

            using var conn = pgConnectionFactory.GetPgConnection();
            var comm = conn.CreateCommand();
            comm.CommandText = @"
                INSERT INTO provider (name, code)
                VALUES (@Name, @Code)

                RETURNING id, name, code
            ";

            comm.Parameters.AddWithValue("@Name", name);
            comm.Parameters.AddWithValue("@Code", code);

            await conn.OpenAsync();

            using var reader = await comm.ExecuteReaderAsync();

            if (await reader.ReadAsync())
                return MapToProvider(reader);

            return null;
        }

        private static Provider MapToProvider(NpgsqlDataReader reader)
        {
            var Id = (int)reader[0];
            var Name = reader.GetString(1);
            var Code = reader.GetString(2);

            return new Provider(Id, Name, Code);
        }

        private async Task<bool> ExistsByCodeAsync(string code)
        {
            var sqlString = "SELECT EXISTS(SELECT 1 FROM provider WHERE code = @p0)";
            return await pgConnectionFactory.ExecuteScalarAsync<bool>(sqlString, [code]);
        }
    }
}
