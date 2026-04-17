using Backend.Databases.Postgres;
using Backend.Models.Context.Page;
using Backend.Models.Context.Page.Contracts;
using Npgsql;

namespace Backend.Repositories.Pages
{
    public class PagesRepository(IPgConnectionFactory pgConnectionFactory) : IPagesRepository
    {
        private const string _pageMapping = "id, page_name, page_type, page_url, site_id, parent_id, page_content, page_meta";

        public async Task<IEnumerable<Page>> GetByParentIdAsync(GetByParentIdRequest request)
        {
            var result = new List<Page>();

            using (var connection = pgConnectionFactory.GetPgConnection())
            {
                var comm = connection.CreateCommand();
                comm.CommandText = @$"
                    SELECT {_pageMapping}
                    FROM content_page
                    WHERE parent_id IS NOT DISTINCT FROM @ParentId AND site_id = @SiteId
                ";

                comm.Parameters.AddWithValue("@SiteId", request.SiteId);
                comm.Parameters.AddWithValue("@ParentId", request.ParentId as object ?? DBNull.Value);

                await connection.OpenAsync();

                using var reader = comm.ExecuteReader();
                while (reader.Read())
                {
                    result.Add(MapToPage(reader));
                }
            }

            return result;
        }

        public async Task<Page?> UpdateAsync(UpdatePageRequest request)
        {
            using var connection = pgConnectionFactory.GetPgConnection();
            var comm = connection.CreateCommand();
            comm.CommandText = $@"
                UPDATE content_page SET 
                    page_name = @Name
                   ,page_type = @Type
                   ,page_content = @Content
                   ,page_meta = @Meta
                WHERE id = @Id

                RETURNING {_pageMapping}
            ";

            comm.Parameters.AddWithValue("@Id", request.Id);
            comm.Parameters.AddWithValue("@Type", request.Type);
            comm.Parameters.AddWithValue("@Name", request.Name);
            comm.Parameters.AddWithValue("@Content", NpgsqlTypes.NpgsqlDbType.Jsonb, request.Content as object ?? DBNull.Value);
            comm.Parameters.AddWithValue("@Meta", NpgsqlTypes.NpgsqlDbType.Jsonb, request.Meta as object ?? DBNull.Value);

            await connection.OpenAsync();

            using var reader = await comm.ExecuteReaderAsync();

            if (await reader.ReadAsync())
                return MapToPage(reader);

            return null;
        }

        public async Task<string?> UpdateAliasAsync(UpdatePageUrlRequest request)
        {
            using var connection = pgConnectionFactory.GetPgConnection();
            var comm = connection.CreateCommand();
            comm.CommandText = @"
                UPDATE content_page SET 
                    page_url = @Url
                WHERE id = @Id

                RETURNING page_url
            ";

            comm.Parameters.AddWithValue("@Id", request.Id);
            comm.Parameters.AddWithValue("@Url", request.Url);

            await connection.OpenAsync();

            using var reader = await comm.ExecuteReaderAsync();

            if (await reader.ReadAsync())
                return reader.GetString(0);

            return null;
        }

        public async Task<Page?> CreateAsync(CreatePageRequest request)
        {
            using var connection = pgConnectionFactory.GetPgConnection();
            var comm = connection.CreateCommand();
            comm.CommandText = @$"
                INSERT INTO content_page (
                     page_name
                    ,page_type
                    ,page_url
                    ,site_id
                    ,parent_id
                    ,page_content
                    ,page_meta
                )
                VALUES (
                     @Name
                    ,@Type                    
                    ,@Url
                    ,@SiteId
                    ,@ParentId
                    ,@Content
                    ,@Meta
                )

                RETURNING {_pageMapping}
            ";

            comm.Parameters.AddWithValue("@Type", request.Type);
            comm.Parameters.AddWithValue("@Name", request.Name);
            comm.Parameters.AddWithValue("@Url", request.Url);
            comm.Parameters.AddWithValue("@SiteId", request.SiteId);
            comm.Parameters.AddWithValue("@ParentId", request.ParentId as object ?? DBNull.Value);
            comm.Parameters.AddWithValue("@Content", NpgsqlTypes.NpgsqlDbType.Jsonb, JsonNullIfEmpty(request.Content));
            comm.Parameters.AddWithValue("@Meta", NpgsqlTypes.NpgsqlDbType.Jsonb, JsonNullIfEmpty(request.Meta));

            await connection.OpenAsync();

            using var reader = await comm.ExecuteReaderAsync();

            if (await reader.ReadAsync())
                return MapToPage(reader);

            return null;
        }

        private static Page MapToPage(NpgsqlDataReader reader)
        {
            var id = reader.GetInt32(0);
            var name = reader.GetString(1);
            var type = reader.GetString(2);
            var url = reader.GetString(3);
            var siteId = reader.GetInt32(4);
            var parentId = reader[6] as int?;
            var content = reader[6] as string;
            var meta = reader[6] as string;

            return new(id, name, type, url, siteId, parentId, content, meta);
        }

        public async Task<bool> PageExistsAsync(int siteId, int? parentId, string url)
        {
            var sqlString = @"
                SELECT EXISTS(
                    SELECT 1 FROM content_page
                    WHERE site_id = @p0
                        AND parent_id IS NOT DISTINCT FROM @p1
                        AND page_url = @p2
                )";

            return await pgConnectionFactory.ExecuteScalarAsync<bool>(sqlString, [siteId, parentId, url]);
        }

        private static object JsonNullIfEmpty(string? data) => string.IsNullOrWhiteSpace(data) ? DBNull.Value : data;
    }
}
