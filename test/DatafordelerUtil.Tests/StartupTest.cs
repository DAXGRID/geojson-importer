using DatafordelerUtil.SqlServer;
using Microsoft.Extensions.Logging.Abstractions;

namespace DatafordelerUtil.Tests;

public sealed class StartupTest : IClassFixture<MsSqlDatabaseFixture>
{
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Start_insert_process_ms_sql()
    {
        var settings = new Settings(
            connectionString: MsSqlDatabaseFixture.TestConnectionString,
            imports: new List<ImportSetting>
            {
                new ImportSetting(
                    schemaName: "dbo",
                    layerName: "jordstykke",
                    filePath: TestUtil.AbsolutePath("Data/jordstykke.geojson"))
            });

        var database = new SqlServerDatafordelerDatabase(
            settings: settings,
            logger: new NullLogger<SqlServerDatafordelerDatabase>());

        var startup = new Startup(
            logger: new NullLogger<Startup>(),
            settings: settings,
            datafordelerDatabase: database);

        await startup.StartAsync().ConfigureAwait(false);
    }
}