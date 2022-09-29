using Microsoft.Data.SqlClient;

namespace DatafordelerUtil.SqlServer;

internal sealed class SqlServerDatafordelerDatabase : IDatafordelerDatabase
{
    public async Task Execute(string sql)
    {
        using var connection = new SqlConnection();
        using var cmd = new SqlCommand(sql);
        await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
    }

    public async Task CreateTable(DynamicTableDescription desc)
    {
        var sql = SqlServerDynamicSchemaBuilder.Create(desc);
        using var connection = new SqlConnection();
        using var cmd = new SqlCommand(sql);
        await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
    }

    public async Task<bool> TableExists(string tableName, string? schemaName)
    {
        var sql = $@" SELECT object_id FROM sys.tables WHERE name = '{tableName}'";
        if (schemaName is not null)
        {
            sql += " AND SCHEMA_NAME(schema_id) = 'dbo'";
        }

        using var connection = new SqlConnection();
        using var cmd = new SqlCommand(sql);
        var result = await cmd.ExecuteScalarAsync().ConfigureAwait(false);

        return result is not null;
    }
}
