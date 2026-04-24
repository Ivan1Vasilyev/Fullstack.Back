using Backend.Databases.Postgres;
using Backend.Models.Context.Phone;
using Backend.Models.Context.Phone.Contracts;
using Npgsql;

namespace Backend.Repositories.Phones
{
    public class PhonesRepository(IPgConnectionFactory pgConnectionFactory) : IPhonesRepository
    {
        public async Task<IEnumerable<Phone>> GetBySiteIdAsync(int siteId)
        {
            var result = new List<Phone>();

            using var connection = pgConnectionFactory.GetPgConnection();
            var comm = connection.CreateCommand();
            comm.CommandText = @$"
                SELECT 
                     id
                    ,label
                    ,link
                    ,name
                    ,role
                    ,site_id
                    ,COALESCE(array_agg(pt.tag_id), ARRAY[]::integer[]) AS city_tag_ids
                FROM content_phone p
                LEFT JOIN provider_content_phone_to_tag pt
                    ON id = pt.phone_id
                WHERE p.site_id = @SiteId
                GROUP BY id, label, link, name, role, site_id
            ";

            comm.Parameters.AddWithValue("@SiteId", siteId);

            await connection.OpenAsync();

            using var reader = comm.ExecuteReader();
            while (reader.Read())
            {
                result.Add(MapToPhone(reader));
            }

            return result;
        }

        public async Task<Phone?> CreateAsync(CreatePhoneRequest request)
        {
            using var connection = pgConnectionFactory.GetPgConnection();
            using var transaction = connection.BeginTransaction();

            var comm = connection.CreateCommand();
            comm.Transaction = transaction;

            comm.CommandText = @$"
                WITH new_phone AS (
                   INSERT INTO content_phone (label, link, name, role, site_id) 
                   VALUES (
                       @Label,
                       @Link,
                       @Name,
                       @Role,
                       @SiteId
                   )
                   RETURNING id, label, link, name, role, site_id
                ),
                insert_links AS (
                    INSERT INTO provider_content_phone_to_tag (phone_id, tag_id)
                    SELECT np.id, unnest(@CityTagIds)
                    FROM new_phone np
                    RETURNING phone_id, tag_id
                )
                SELECT 
                    np.id,
                    np.label, 
                    np.link, 
                    np.name,
                    np.role,
                    np.site_id,
                    array_agg(il.tag_id)
                FROM new_phone np
                LEFT JOIN insert_links il ON np.id = il.phone_id
                GROUP BY np.id, np.label, np.link, np.name, np.role, np.site_id;
            ";

            comm.Parameters.AddWithValue("@Label", request.Label);
            comm.Parameters.AddWithValue("@Link", request.Link);
            comm.Parameters.AddWithValue("@Name", request.Link);
            comm.Parameters.AddWithValue("@Role", request.Role);
            comm.Parameters.AddWithValue("@SiteId", request.SiteId);
            comm.Parameters.AddWithValue("@CityTagIds", request.CityTagIds);

            Phone? result = null;
            await connection.OpenAsync();

            using (var reader = comm.ExecuteReader())
            {
                if (reader.Read())
                {
                    result = MapToPhone(reader);
                }
            }

            await transaction.CommitAsync();

            return result;
        }

        public async Task<Phone?> UpdateAsync(Phone request)
        {
            using var connection = pgConnectionFactory.GetPgConnection();
            using var transaction = connection.BeginTransaction();

            var comm = connection.CreateCommand();
            comm.Transaction = transaction;

            comm.CommandText = @"
                WITH updated_phone AS (
                    UPDATE content_phone 
                    SET label = @Label,
                        link = @Link,
                        name = @Name,
                        role = @Role,
                        site_id = @SiteId
                    WHERE id = @Id
                    RETURNING id, label, link, name, site_id
                ),
                deleted_links AS (
                    DELETE FROM provider_content_phone_to_tag 
                    WHERE phone_id = @Id
                    RETURNING phone_id, tag_id
                ),
                inserted_links AS (
                    INSERT INTO provider_content_phone_to_tag (site_id, phone_id, tag_id)
                    SELECT @SiteId, @Id, unnest(@TagIds::integer[])
                    WHERE array_length(@TagIds, 1) > 0
                    RETURNING phone_id, tag_id
                )
                SELECT 
                    up.id
                    up.label,
                    up.link,
                    up.name,                    
                    up.role,
                    up.site_id,
                    array_agg(il.tag_id) AS city_tag_ids
                FROM updated_phone up
                LEFT JOIN inserted_links il ON up.id = il.phone_id
                GROUP BY up.id, up.label, up.link, up.name, up.role, up.site_id;
            ";

            comm.Parameters.AddWithValue("@Id", request.Id);
            comm.Parameters.AddWithValue("@Label", request.Label);
            comm.Parameters.AddWithValue("@Link", request.Link);
            comm.Parameters.AddWithValue("@Name", request.Name);
            comm.Parameters.AddWithValue("@Role", request.Role);
            comm.Parameters.AddWithValue("@SiteId", request.SiteId);
            comm.Parameters.AddWithValue("@TagIds", request.CityTagIds);

            Phone? result = null;
            await connection.OpenAsync();

            using (var reader = comm.ExecuteReader())
            {
                if (reader.Read())
                {
                    result = MapToPhone(reader);
                }
            }

            await transaction.CommitAsync();

            return result;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var sqlString = "DELETE FROM content_phone WHERE id = @p0";
            var rowsAffected = await pgConnectionFactory.ExecuteScalarAsync<int>(sqlString, [id]);
            return rowsAffected > 0;
        }

        private static Phone MapToPhone(NpgsqlDataReader reader)
        {
            var id = reader.GetInt32(0);
            var label = reader.GetString(1);
            var link = reader.GetString(2);
            var name = reader.GetString(3);
            var role = reader.GetInt32(4);
            var siteId = reader.GetInt32(5);
            var cityTags = (int[])reader.GetValue(6);

            return new(id, label, link, name, role, siteId, cityTags);
        }
    }
}
