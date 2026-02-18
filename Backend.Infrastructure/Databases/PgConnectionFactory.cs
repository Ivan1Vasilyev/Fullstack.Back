using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Backend.Infrastructure.Databases
{
    public class PgConnectionFactory: IPgConnectionFactory
    {
        private readonly string _cs;
        private readonly Dictionary<string, string> _csStringsByName;

        public PgConnectionFactory(IConfiguration config)
        {
            _cs = config.GetConnectionString("DefaultConnection");

            var csSection = config.GetSection("ConnectionStrings");

            _csStringsByName = [];

            foreach (var sub in csSection.GetChildren())
            {
                _csStringsByName[sub.Key] = sub.Value;
            }
        }

        public async Task ExecuteNonQueryAsync(string sql, params object?[] parameters)
        {
            using var conn = GetPgConnection();
            var comm = conn.CreateCommand();
            comm.CommandText = sql;
            CreateParameters(comm, parameters);
            await conn.OpenAsync();
            await comm.ExecuteNonQueryAsync();
        }

        private void CreateParameters(NpgsqlCommand comm, params object?[] parameters)
        {
            if (parameters == null) return;

            for (var i = 0; i < parameters.Length; i++)
            {
                comm.Parameters.AddWithValue($"@p{i}", parameters[i] ?? DBNull.Value);
            }
        }

        public async Task<T?> ExecuteScalarAsync<T>(string csName, string sql, params object?[] parameters)
        {
            using (var conn = GetPgConnection(_csStringsByName[csName]))
            {
                var comm = conn.CreateCommand();
                comm.CommandText = sql;
                CreateParameters(comm, parameters);
                await conn.OpenAsync();

                var result = await comm.ExecuteScalarAsync();
                if (result is T value)
                    return value;

                return default;
            }
        }

        // Не используем для объектов, Т - примитив
        public async Task<T?> ExecuteScalarAsync<T>(string sql, params object?[] parameters)
        {
            using var conn = GetPgConnection();
            var comm = conn.CreateCommand();
            comm.CommandText = sql;
            CreateParameters(comm, parameters);

            await conn.OpenAsync();

            var result = await comm.ExecuteScalarAsync();
            if (result is T value)
                return value;

            return default;
        }

        public NpgsqlConnection GetPgConnection()
        {
            return new NpgsqlConnection(_cs);
        }

        public NpgsqlConnection GetPgConnection(string csString)
        {
            return new NpgsqlConnection(csString);
        }
    }
}
