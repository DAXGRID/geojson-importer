using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.SqlServer.Types;
using System.Data;
using System.Data.SqlTypes;
using System.Globalization;

namespace GeoJsonImporter.SqlServer;

internal sealed class SqlServerDatafordelerDatabase : IDatafordelerDatabase
{
    private readonly Settings _settings;
    private readonly ILogger<SqlServerDatafordelerDatabase> _logger;

    public SqlServerDatafordelerDatabase(
        Settings settings,
        ILogger<SqlServerDatafordelerDatabase> logger)
    {
        _settings = settings;
        _logger = logger;
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
        cmd.CommandTimeout = 60 * 10;

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
        string tableName,
        IEnumerable<GeoJsonFeature> features,
        IReadOnlyDictionary<string, string> fieldNameMappings,
        string? schemaName)
    {
        if (features is null || !features.Any())
        {
            throw new ArgumentException(
                $"{nameof(features)} cannot be null, or have a count less than 1.");
        }

        using var table = new DataTable();
        table.TableName = tableName;

        var exampleFeature = features.First();
        foreach (var property in features.First().Properties)
        {
            fieldNameMappings.TryGetValue(property.Key, out var propertyKeyMapping);
            table.Columns.Add(propertyKeyMapping ?? property.Key, typeof(string));
        }

        if (exampleFeature.Geometry is not null)
        {
            table.Columns.Add("coord", typeof(SqlGeometry));
        }

        foreach (var row in features
                 .Select(x => CreateFeatureRow(table.NewRow(), x, fieldNameMappings)))
        {
            table.Rows.Add(row);
        }

        using var connection = new SqlConnection(_settings.ConnectionString);

        using var bulkInsert = new SqlBulkCopy(connection);
        bulkInsert.DestinationTableName = schemaName is not null
            ? $"[{schemaName}].[{tableName}]"
            : $"[{tableName}]";

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
        cmd.CommandTimeout = 60 * 10;

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
