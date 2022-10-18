using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;

namespace GeoJsonImporter.Tests;

internal class MsSqlDatabaseFixture : IAsyncLifetime
{
    private const string MasterDatabaseName = "master";
    private const string TestDatabaseName = "test_database";
    private const string TestDatabaseSchemaName = "test_schema";

    public static string MasterConnectionString =>
        CreateConnectionString(MasterDatabaseName);
    public static string TestConnectionString =>
        CreateConnectionString(TestDatabaseName);

    public async Task InitializeAsync()
    {
        await DropDatabase(
            MasterConnectionString,
            TestDatabaseName)
            .ConfigureAwait(true);

        await SetupDatabase(
            MasterConnectionString,
            TestDatabaseName).ConfigureAwait(false);

        await SetupSchema(TestConnectionString, TestDatabaseSchemaName);
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    private static async Task SetupDatabase(
        string connectionString,
        string database)
    {
        var createDatabaseSql = $"CREATE DATABASE {database}";

        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync().ConfigureAwait(false);

        new Server(new ServerConnection(connection))
            .ConnectionContext.ExecuteNonQuery(createDatabaseSql);
    }

    private static async Task SetupSchema(string connectionString, string schemaName)
    {
        var createSchemaSql = $"CREATE SCHEMA {schemaName}";

        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync().ConfigureAwait(false);

        new Server(new ServerConnection(connection))
            .ConnectionContext.ExecuteNonQuery(createSchemaSql);
    }

    private static async Task DropDatabase(
        string connectionString,
        string database)
    {
        var deleteDatabaseSql = $@"
            IF DB_ID('{database}') IS NOT NULL
              BEGIN
                ALTER DATABASE {database} SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                DROP DATABASE {database};
            END;";

        using var connection = new SqlConnection(connectionString);
        using var cmd = new SqlCommand(deleteDatabaseSql, connection);

        await connection.OpenAsync().ConfigureAwait(false);
        await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
    }

    private static string CreateConnectionString(string initialCatalog)
        => new SqlConnectionStringBuilder
        {
            DataSource = "localhost",
            UserID = "sa",
            Password = "myAwesomePassword1",
            InitialCatalog = initialCatalog,
            Encrypt = false
        }.ConnectionString;
}
