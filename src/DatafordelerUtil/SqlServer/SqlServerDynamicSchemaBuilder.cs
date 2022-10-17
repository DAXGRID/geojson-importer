namespace DatafordelerUtil.SqlServer;

internal static class SqlServerDynamicSchemaBuilder
{
    public static string Create(DynamicTableDescription desc) =>
        $"{CreateTable(desc)}({CreateColumns(desc)})";

    private static string CreateTable(DynamicTableDescription dynamicTableDescription) =>
        $"CREATE TABLE [{dynamicTableDescription.Schema}].[{dynamicTableDescription.Name}]";

    private static string CreateColumns(DynamicTableDescription desc) =>
        string.Join(",", desc.Columns.Select(CreateColumn));

    private static string CreateColumn(DynamicColumnDescription desc) => desc.PrimaryKey
        ? $"[{desc.Name}] {MapColumnType(desc.ColumnType)} NOT NULL PRIMARY KEY"
        : $"[{desc.Name}] {MapColumnType(desc.ColumnType)} NULL";

    private static string MapColumnType(ColumnType columnType) => columnType switch
    {
        ColumnType.Guid => "[UNIQUEIDENTIFIER]",
        ColumnType.Int => "[int]",
        ColumnType.Geometry => "[geometry]",
        ColumnType.String => "[nvarchar](MAX)",
        _ => throw new ArgumentException(
            $"Could not handle type {Enum.GetName(columnType)}")
    };
}
