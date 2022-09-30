using Microsoft.Data.SqlClient;

namespace DatafordelerUtil.SqlServer;

internal sealed class SqlServerDatafordelerDatabase : IDatafordelerDatabase
{
    private readonly Settings _settings;

    public SqlServerDatafordelerDatabase(Settings settings)
    {
        _settings = settings;
    }

    public async Task Execute(string sql)
    {
        using var connection = new SqlConnection(_settings.ConnectionString);
        using var cmd = new SqlCommand(sql, connection);

        await connection.OpenAsync().ConfigureAwait(false);
        await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
    }

    public async Task CreateTable(DynamicTableDescription desc)
    {
        var sql = SqlServerDynamicSchemaBuilder.Create(desc);

        using var connection = new SqlConnection(_settings.ConnectionString);
        using var cmd = new SqlCommand(sql, connection);

        await connection.OpenAsync().ConfigureAwait(false);
        await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
    }

    public async Task<bool> TableExists(string tableName, string? schemaName)
    {
        var sql = $@"SELECT object_id FROM sys.tables WHERE name = '{tableName}'";
        if (schemaName is not null)
        {
            sql += $" AND SCHEMA_NAME(schema_id) = '{schemaName}'";
        }

        using var connection = new SqlConnection(_settings.ConnectionString);
        using var cmd = new SqlCommand(sql, connection);

        await connection.OpenAsync().ConfigureAwait(false);
        var result = await cmd.ExecuteScalarAsync().ConfigureAwait(false);

        return result is not null;
    }
}
