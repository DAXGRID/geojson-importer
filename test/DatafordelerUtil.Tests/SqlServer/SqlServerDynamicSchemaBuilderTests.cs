using DatafordelerUtil.SqlServer;

namespace DatafordelerUtil.Tests.SqlServer;

public sealed class SqlServerDynamicSchemaBuilderTests
{
    [Fact]
    public void Should_create_dynamic_sql_server_schema()
    {
        var dynamicTableDescription = new DynamicTableDescription(
            schema: "dbo",
            name: "jordstykke",
            key: "id",
            columns: new List<DynamicColumnDescription>
            {
                new("id", ColumnType.Int, true),
                new("forretningshaendelse", ColumnType.String, false),
                new("senestesaglokalid", ColumnType.String, false),
                new("forretningsproces", ColumnType.String, false),
                new("coord", ColumnType.Geometry, false),
            }
        );

        var result = SqlServerDynamicSchemaBuilder.Create(dynamicTableDescription);

        var expected = @"
CREATE TABLE [dbo].[jordstykke](
[id] [int] NOT NULL PRIMARY KEY,
[forretningshaendelse] [nvarchar](MAX) NULL,
[senestesaglokalid] [nvarchar](MAX) NULL,
[forretningsproces] [nvarchar](MAX) NULL,
[coord] [geometry] NULL)";

        // We split and concat to remove newlines and spaces,
        // the reason is that we want the expected to be easier to read,
        // but we need it to be one long string in the result.
        result.Should().Be(string.Concat(expected.Trim().Split('\n')));
    }
}
