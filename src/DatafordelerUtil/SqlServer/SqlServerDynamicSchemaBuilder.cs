namespace DatafordelerUtil.SqlServer;

internal static class SqlServerDynamicSchemaBuilder
{
    public static string Create(DynamicTableDescription desc) =>
        $"{CreateTable(desc)}({CreateColumns(desc)})";

    private static string CreateColumns(DynamicTableDescription desc) =>
        string.Join(",", desc.Columns.Select(CreateColumn));

    private static string CreateColumn(DynamicColumnDescription desc) =>
        $"[{desc.Name}] [nvarchar](n) NULL";

    private static string CreateTable(DynamicTableDescription dynamicTableDescription) =>
        $"CREATE TABLE [{dynamicTableDescription.Schema}].[{dynamicTableDescription.Name}]";
}
