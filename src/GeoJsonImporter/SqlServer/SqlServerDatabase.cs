using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.SqlServer.Types;
using System.Data;
using System.Data.SqlTypes;
using System.Globalization;

namespace GeoJsonImporter.SqlServer;

internal sealed class SqlServerDatabase : IDatafordelerDatabase
{
    private const int DEFAULT_COMMAND_TIMEOUT = 60 * 60;
    private readonly Settings _settings;
    private readonly ILogger<SqlServerDatabase> _logger;

    public SqlServerDatabase(
        Settings settings,
        ILogger<SqlServerDatabase> logger)
    {
        _settings = settings;
        _logger = logger;
    }

    public async Task Execute(string sql)
    {
        using var connection = new SqlConnection(_settings.ConnectionString);
        using var cmd = new SqlCommand(sql, connection);

        cmd.CommandTimeout = DEFAULT_COMMAND_TIMEOUT;

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

    public async Task DeleteTable(string tableName, string? schemaName = null)
    {
        var schema = schemaName is not null ? $"[{schemaName}]." : "";
        var sql = $"DROP TABLE {schema}[{tableName}]";

        using var connection = new SqlConnection(_settings.ConnectionString);
        using var cmd = new SqlCommand(sql, connection);

        await connection.OpenAsync().ConfigureAwait(false);
        await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
    }

    public async Task Merge(
        DynamicTableDescription target,
        DynamicTableDescription source)
    {
        var sql = SqlServerDynamicMergeBuilder.Build(target, source);

        using var connection = new SqlConnection(_settings.ConnectionString);
        using var cmd = new SqlCommand(sql, connection);
        cmd.CommandTimeout = DEFAULT_COMMAND_TIMEOUT;

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

    public async Task BulkImportGeoJsonFeatures(
        DynamicTableDescription tableDescription,
        IReadOnlyList<GeoJsonFeature> features,
        IReadOnlyDictionary<string, string> fieldNameMappings)
    {
        if (features is null || !features.Any())
        {
            throw new ArgumentException(
                $"{nameof(features)} cannot be null, or have a count less than 1.");
        }

        using var table = new DataTable();
        table.TableName = tableDescription.Name;

        foreach (var property in tableDescription.Columns)
        {
            var columnType = property.ColumnType switch
            {
                ColumnType.Geometry => typeof(SqlGeometry),
                _ => typeof(string)
            };

            fieldNameMappings.TryGetValue(property.Name, out var propertyKeyMapping);

            table.Columns.Add(propertyKeyMapping ?? property.Name, columnType);
        }

        foreach (var row in features
                 .Select(x => CreateFeatureRow(table.NewRow(), x, fieldNameMappings)))
        {
            table.Rows.Add(row);
        }

        using var connection = new SqlConnection(_settings.ConnectionString);

        using var bulkInsert = new SqlBulkCopy(connection);
        bulkInsert.DestinationTableName = tableDescription.Schema is not null
            ? $"[{tableDescription.Schema}].[{tableDescription.Name}]"
            : $"[{tableDescription.Name}]";

        await connection.OpenAsync().ConfigureAwait(false);
        await bulkInsert.WriteToServerAsync(table).ConfigureAwait(false);
    }

    public async Task CreateSpatialIndex(string tableName, string? schemaName)
    {
        if (_settings.SpartialIndexStatement is null)
        {
            throw new InvalidOperationException(
                "No spatial index statement has been set in the config.");
        }

        var sql = _settings.SpartialIndexStatement.Replace(
            "{table_name}", tableName, true, CultureInfo.InvariantCulture);

        if (schemaName is not null)
        {
            sql = sql.Replace(
                "{schema_name}",
                schemaName,
                true,
                CultureInfo.InvariantCulture);
        }

        using var connection = new SqlConnection(_settings.ConnectionString);
        using var cmd = new SqlCommand(sql, connection);
        cmd.CommandTimeout = DEFAULT_COMMAND_TIMEOUT;

        await connection.OpenAsync().ConfigureAwait(false);
        await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
    }

    public async Task<bool> IndexExists(
        string indexName,
        string tableName,
        string? schemaName)
    {
        var objectId = schemaName is null
            ? tableName
            : $"{schemaName}.{tableName}";

        var sql = $@"
SELECT object_id
FROM sys.indexes
WHERE name='{indexName}' AND object_id = OBJECT_ID('{objectId}')";

        using var connection = new SqlConnection(_settings.ConnectionString);
        using var cmd = new SqlCommand(sql, connection);

        await connection.OpenAsync().ConfigureAwait(false);

        var result = await cmd.ExecuteScalarAsync().ConfigureAwait(false);

        return result is not null;
    }

    private DataRow CreateFeatureRow(
        DataRow row,
        GeoJsonFeature feature,
        IReadOnlyDictionary<string, string> fieldNameMappings)
    {
        foreach (var property in feature.Properties)
        {
            fieldNameMappings.TryGetValue(property.Key, out var propertyKeyMapping);
            row[propertyKeyMapping ?? property.Key] = GetDBValue(property.Value);
        }

        if (feature?.Geometry?.Coordinates is not null)
        {
            try
            {
                row["coord"] = SqlGeometry.STGeomFromText(
                    new SqlChars(feature.Geometry.AsText()),
                    _settings.Srid);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("{Message}", ex.Message);
                row["coord"] = SqlGeometry.Null;
            }
        }

        return row;
    }

    private static object GetDBValue(object? o)
    {
        return o ?? (object)DBNull.Value;
    }
}
