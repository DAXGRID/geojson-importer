using DatafordelerUtil.SqlServer;

namespace DatafordelerUtil.Tests.SqlServer;

public sealed class SqlServerDynamicSchemaBuilderTests
{
    [Fact]
    public void Should_create_dynamic_sql_server_schema()
    {
        var dynamicTableDescription = new DynamicTableDescription(
            name: "jordstykke",
            columns: new List<DynamicColumnDescription>
            {
                new("id", ColumnType.String, 255, true),
                new("forretningshaendelse", ColumnType.String, 255, true),
                new("senestesaglokalid", ColumnType.String, 255, true),
                new("forretningsproces", ColumnType.String, 255, true),
            }
        );

        var result = SqlServerDynamicSchemaBuilder.Create(dynamicTableDescription);

        var expected = @"
CREATE TABLE [dbo].[jordstykke](
[id] [varchar](255) NULL,
[forretningshaendelse] [varchar](255) NULL,
[senestesaglokalid] [varchar](255) NULL,
[forretningsproces] [varchar](255) NULL)";

        // We split and concat to remove newlines and spaces,
        // the reason is that we want the expected to be easier to read,
        // but we need it to be one long string in the result.
        result.Should().Be(string.Concat(expected.Trim().Split('\n')));
    }
}
