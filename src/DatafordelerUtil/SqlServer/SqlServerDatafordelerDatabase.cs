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

        foreach (var row in features.Select(x => CreateFeatureRow(table.NewRow(), x)))
        {
            table.Rows.Add(row);
        }

        using var connection = new SqlConnection(_settings.ConnectionString);

        using var bulkInsert = new SqlBulkCopy(connection);
        bulkInsert.DestinationTableName = tableName;

        await connection.OpenAsync().ConfigureAwait(false);
        await bulkInsert.WriteToServerAsync(table).ConfigureAwait(false);
    }

    private DataRow CreateFeatureRow(DataRow row, GeoJsonFeature feature)
    {
        foreach (var property in feature.Properties)
        {
            row[property.Key] = GetDBValue(property.Value);
        }

        if (feature?.Geometry?.Coordinates is not null)
        {
            try
            {
                if (feature.Geometry.Type == "Point")
                {
                    var coord = (double[])feature.Geometry.Coordinates;

                    var point = new Point(coord[0], coord[1])
                    {
                        SRID = 25832
                    };

                    row["coord"] = SqlGeometry.STGeomFromText(
                        new SqlChars(point.AsText()), point.SRID);
                }
                else if (feature.Geometry.Type == "LineString")
                {
                    var coordinates = ((double[][])feature.Geometry.Coordinates)
                        .Select(x => new Coordinate(x[0], x[1]))
                        .ToArray();

                    var lineString = new LineString(coordinates)
                    {
                        SRID = 25832
                    };

                    row["coord"] = SqlGeometry.STGeomFromText(
                        new SqlChars(lineString.AsText()), lineString.SRID);
                }
                else if (feature.Geometry.Type == "Polygon")
                {
                    var coordinates = ((double[][][])feature.Geometry.Coordinates)
                        .SelectMany(x => x.Select(y => new Coordinate(y[0], y[1])))
                        .ToArray();

                    var polygon = new Polygon(new LinearRing(coordinates))
                    {
                        SRID = 25832
                    };

                    row["coord"] = SqlGeometry.STGeomFromText(
                        new SqlChars(polygon.AsText()), polygon.SRID);
                }
                else
                {
                    throw new InvalidOperationException(
                        $"Cannot handle geometry of type {feature.Geometry.Type}.");
                }
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("{Message}", ex.Message);
                row["coord"] = SqlGeometry.Null;
            }
        }
        else
        {
            row["coord"] = SqlGeometry.Null;
        }

        return row;
    }

    private static object GetDBValue(object? o)
    {
        return o ?? (object)DBNull.Value;
    }
}
