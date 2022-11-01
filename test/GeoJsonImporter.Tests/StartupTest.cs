using GeoJsonImporter.SqlServer;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging.Abstractions;

namespace GeoJsonImporter.Tests;

public sealed class StartupTest : IClassFixture<MsSqlDatabaseFixture>
{
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Start_insert_process_ms_sql()
    {
        var settings = new Settings(
            srid: 25832,
            spartialIndexStatement: "CREATE SPATIAL INDEX [coord_sidx] ON [{schema_name}].[{table_name}] (coord) USING  GEOMETRY_GRID WITH (BOUNDING_BOX =(400000, 6000000, 900000, 6450000), GRIDS =(LEVEL_1 = MEDIUM,LEVEL_2 = MEDIUM,LEVEL_3 = MEDIUM,LEVEL_4 = MEDIUM), CELLS_PER_OBJECT = 16) ON [PRIMARY]",
            connectionString: MsSqlDatabaseFixture.TestConnectionString,
            imports: new List<ImportSetting>
            {
                new ImportSetting(
                    schemaName: "test_schema",
                    tableName: "jordstykke",
                    keyFieldName: "id",
                    filePath: TestUtil.AbsolutePath("Data/jordstykke.geojson"),
                    fieldNameMappings: null),
                new ImportSetting(
                    schemaName: "test_schema",
                    tableName: "postnummer",
                    keyFieldName: "nr",
                    filePath: TestUtil.AbsolutePath("Data/postnummer.geojson"),
                    fieldNameMappings: null),
                new ImportSetting(
                    schemaName: "test_schema",
                    tableName: "adgangsadresse",
                    keyFieldName: "id",
                    filePath: TestUtil.AbsolutePath("Data/adgangsadresse.geojson"),
                    fieldNameMappings: new Dictionary<string, string>
                    {
                        { "adressepunktændringsdato", "dato"},
                        { "postnr", "postnummer"}
                    })});

        var database = new SqlServerDatafordelerDatabase(
            settings: settings,
            logger: new NullLogger<SqlServerDatafordelerDatabase>());

        var startup = new Startup(
            logger: new NullLogger<Startup>(),
            settings: settings,
            datafordelerDatabase: database);

        await startup.StartAsync().ConfigureAwait(false);

        var columns = await RetrieveAllColumns(
            MsSqlDatabaseFixture.TestConnectionString,
            "jordstykke",
            "test_schema");

        // It's important that the count matches exact, so we know all has been inserted.
        columns.Count.Should().Be(1583);

        // Temporary table should be cleaned up.
        var exists = await TableExists(
            MsSqlDatabaseFixture.TestConnectionString,
            "jordstykke_tmp",
            "test_schema");

        exists.Should().BeFalse();
    }

    private async Task<List<IReadOnlyDictionary<string, object>>>
        RetrieveAllColumns(string connectionString, string tableName, string schema)
    {
        using var connection = new SqlConnection(MsSqlDatabaseFixture.TestConnectionString);
        using var cmd = new SqlCommand($"SELECT * FROM [{schema}].[{tableName}]", connection);

        var columns = new List<IReadOnlyDictionary<string, object>>();

        await connection.OpenAsync();
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var column = new Dictionary<string, object>();
            for (var i = 0; i < reader.FieldCount; i++)
            {
                column.Add(reader.GetName(i), reader.GetValue(i));
            }

            columns.Add(column);
        }

        return columns;
    }

    private async Task<bool> TableExists(
        string connectionString,
        string tableName,
        string? schemaName)
    {
        var sql = $@"SELECT object_id FROM sys.tables WHERE name = '{tableName}'";
        if (schemaName is not null)
        {
            sql += $" AND SCHEMA_NAME(schema_id) = '{schemaName}'";
        }

        using var connection = new SqlConnection(connectionString);
        using var cmd = new SqlCommand(sql, connection);

        await connection.OpenAsync().ConfigureAwait(false);
        return (await cmd.ExecuteScalarAsync()) is not null;
    }
}
