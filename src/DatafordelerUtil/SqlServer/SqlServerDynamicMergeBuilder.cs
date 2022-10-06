namespace DatafordelerUtil.SqlServer;

internal static class SqlServerDynamicMergeBuilder
{
    internal static string Build(
        DynamicTableDescription target,
        DynamicTableDescription source)
    {
        return @$"
MERGE [{target.Schema}].[{target.Name}] AS Target
USING [{source.Schema}].[{source.Name}] AS Source
ON Source.{source.Key} = Target.{target.Key}
WHEN NOT MATCHED BY Target THEN
INSERT ({string.Join(',', target.Columns.Select(x => x.Name))})
VALUES ({string.Join(',', target.Columns.Select(x => "Source." + x.Name))})
WHEN MATCHED THEN UPDATE SET
{string.Join(",\n", target.Columns.Select(x => $"Target.{x.Name} = Source.{x.Name}"))}
WHEN NOT MATCHED BY Source THEN
DELETE;";
    }
}
