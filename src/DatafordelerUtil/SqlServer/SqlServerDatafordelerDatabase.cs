using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.SqlServer.Types;
using NetTopologySuite.Geometries;
using System.Data;
using System.Data.SqlTypes;

namespace DatafordelerUtil.SqlServer;

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
        IEnumerable<GeoJsonFeature> features)
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
            // TODO make a cleaner implementation so not all of them are strings. :))
            table.Columns.Add(property.Key, typeof(string));
        }

        if (exampleFeature.Geometry is not null)
        {
            table.Columns.Add("coord", typeof(SqlGeometry));
        }

        foreach (var feature in features)
        {
            var row = table.NewRow();

            foreach (var property in feature.Properties)
            {
                row[property.Key] = GetDBValue(property.Value);
            }

            if (feature?.Geometry?.Coordinates is not null)
            {
                var coordinates = ((double[][][])feature.Geometry.Coordinates)
                    .SelectMany(x => x.Select(y => new Coordinate(y[0], y[1])))
                    .ToArray();

                try
                {
                    var polygon = new Polygon(new LinearRing(coordinates))
                    {
                        SRID = 25832
                    };

                    row["coord"] = SqlGeometry.STGeomFromText(
                        new SqlChars(polygon.AsText()), polygon.SRID);
                }
                catch (ArgumentException ex)
                {
                    _logger.LogWarning("{Message}", ex.Message);
                    continue;
                }
            }
            else
            {
                row["coord"] = DBNull.Value;
            }

            table.Rows.Add(row);
        }

        using var connection = new SqlConnection(_settings.ConnectionString);

        using var bulkInsert = new SqlBulkCopy(connection);
        bulkInsert.DestinationTableName = tableName;

        await connection.OpenAsync().ConfigureAwait(false);
        await bulkInsert.WriteToServerAsync(table).ConfigureAwait(false);
    }

    private static object GetDBValue(object o)
    {
        return o ?? (object)DBNull.Value;
    }
}
