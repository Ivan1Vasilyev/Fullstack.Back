using Npgsql;

namespace Backend.Infrastructure.Databases
{
    public interface IPgConnectionFactory
    {
        NpgsqlConnection GetPgConnection();
        NpgsqlConnection GetPgConnection(string csString);
        Task ExecuteNonQueryAsync(string sql, params object[] parameters);
        Task<T?> ExecuteScalarAsync<T>(string csName, string sql, params object[] parameters);
        Task<T?> ExecuteScalarAsync<T>(string sql, params object[] parameters);
    }
}
