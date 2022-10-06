using DatafordelerUtil.SqlServer;

namespace DatafordelerUtil.Tests.SqlServer;

public sealed class SqlServerDynamicMergeBuilderTests
{
    [Fact]
    public void Build_dynamic_merge()
    {
        var target = new DynamicTableDescription(
            schema: "dbo",
            name: "jordstykke",
            key: "id",
            columns: new List<DynamicColumnDescription>
            {
                new("id", ColumnType.String),
                new("forretningshaendelse", ColumnType.String),
                new("senestesaglokalid", ColumnType.String),
                new("forretningsproces", ColumnType.String),
                new("coord", ColumnType.Geometry),
            }
        );

        var source = new DynamicTableDescription(
            schema: "dbo",
            name: "jordstykke_tmp",
            key: "id",
            columns: new List<DynamicColumnDescription>
            {
                new("id", ColumnType.String),
                new("forretningshaendelse", ColumnType.String),
                new("senestesaglokalid", ColumnType.String),
                new("forretningsproces", ColumnType.String),
                new("coord", ColumnType.Geometry),
            }
        );

        var result = SqlServerDynamicMergeBuilder.Build(target, source);

        var expected = @"
MERGE [dbo].[jordstykke] AS Target
USING [dbo].[jordstykke_tmp] AS Source
ON Source.id = Target.id
WHEN NOT MATCHED BY Target THEN
INSERT (id,forretningshaendelse,senestesaglokalid,forretningsproces,coord)
VALUES (Source.id,Source.forretningshaendelse,Source.senestesaglokalid,Source.forretningsproces,Source.coord)
WHEN MATCHED THEN UPDATE SET
Target.id = Source.id,
Target.forretningshaendelse = Source.forretningshaendelse,
Target.senestesaglokalid = Source.senestesaglokalid,
Target.forretningsproces = Source.forretningsproces,
Target.coord = Source.coord
WHEN NOT MATCHED BY Source THEN
DELETE;";

        result.Should().Be(expected);
    }
}
