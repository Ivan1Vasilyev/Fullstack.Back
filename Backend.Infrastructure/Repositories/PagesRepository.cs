using Backend.Application.Interfaces.Repositories;
using Backend.Domain.Common.Exceptions;
using Backend.Domain.Models;
using Backend.Infrastructure.Databases;
using Npgsql;

namespace Backend.Infrastructure.Repositories
{
    public class PagesRepository(IPgConnectionFactory pgConnectionFactory) : IPagesRepository
    {
        public async Task<Page?> CreateAsync(string name, string type, string alias, int siteId, int? parentId, string? content, string? title, string? description)
        {
            var exists = await PageExistsAsync(siteId, parentId, alias);
            if (exists)
                throw new UniqueViolationException("page", new() {
                    { "siteId", siteId },
                    { "parentId", parentId },
                    { "alias", alias }
                });

            using var conn = pgConnectionFactory.GetPgConnection();
            var comm = conn.CreateCommand();
            comm.CommandText = @"
                INSERT INTO page (
                     page_type
                    ,page_name
                    ,page_alias
                    ,site_id
                    ,parent_id
                    ,content
                    ,title
                    ,description
                )
                VALUES (
                     @Type
                    ,@Name                    
                    ,@Alias
                    ,@SiteId
                    ,@ParentId
                    ,@Content
                    ,@Title
                    ,@Description
                )

                RETURNING 
                     id
                    ,page_type
                    ,page_name
                    ,page_alias
                    ,site_id
                    ,parent_id
                    ,content
                    ,title
                    ,description
            ";

            comm.Parameters.AddWithValue("@Type", type);
            comm.Parameters.AddWithValue("@Name", name);
            comm.Parameters.AddWithValue("@Alias", alias);
            comm.Parameters.AddWithValue("@SiteId", siteId);
            comm.Parameters.AddWithValue("@ParentId", parentId as object ?? DBNull.Value);
            comm.Parameters.AddWithValue("@Content", NpgsqlTypes.NpgsqlDbType.Jsonb, content as object ?? DBNull.Value);
            comm.Parameters.AddWithValue("@Title", title as object ?? DBNull.Value);
            comm.Parameters.AddWithValue("@Description", description as object ?? DBNull.Value);

            await conn.OpenAsync();

            using var reader = await comm.ExecuteReaderAsync();

            if (await reader.ReadAsync())
                return MapToPage(reader);

            return null;
        }

        private async Task<bool> PageExistsAsync(int siteId, int? parentId, string alias)
        {
            var sqlString = @"
                SELECT EXISTS(
                    SELECT 1 FROM page
                    WHERE site_id = @p0 AND parent_id = @p1 AND page_alias = @p2
                )";

            return await pgConnectionFactory.ExecuteScalarAsync<bool>(sqlString, [siteId, parentId, alias]);
        }

        public Task<IEnumerable<Page>> GetSitePagesAsync(int id)
        {
            throw new NotImplementedException();
        }

        private Page MapToPage(NpgsqlDataReader reader)
        {
            var id = reader.GetInt32(0);
            var type = reader.GetString(1);
            var name = reader.GetString(2);
            var alias = reader.GetString(3);
            var siteId = reader.GetInt32(4);
            var parentId = reader[5] as int?;
            var content = reader.GetString(6);
            var title = reader[7] as string;
            var description = reader[8] as string;

            return new(id, type, name, alias, siteId, parentId, content, title, description);
        }

        public Task<IEnumerable<Page>> GetChildPagesAsync(int parentId)
        {
            throw new NotImplementedException();
        }
    }
}
