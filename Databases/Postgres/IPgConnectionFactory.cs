using Npgsql;

namespace Backend.Databases.Postgres
{
    public interface IPgConnectionFactory
    {
        Task ExecuteNonQueryAsync(string sql, params object?[] parameters);
        Task<T?> ExecuteScalarAsync<T>(string sql, params object?[] parameters);
        Task<T?> ExecuteScalarAsync<T>(string csName, string sql, params object?[] parameters);
        NpgsqlConnection GetPgConnection();
        NpgsqlConnection GetPgConnection(string csString);
    }
}