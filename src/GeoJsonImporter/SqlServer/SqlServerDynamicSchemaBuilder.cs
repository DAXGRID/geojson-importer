namespace GeoJsonImporter.SqlServer;

internal static class SqlServerDynamicSchemaBuilder
{
    public static string Create(DynamicTableDescription desc) =>
        $"{CreateTable(desc)}({CreateColumns(desc)})";

    private static string CreateTable(DynamicTableDescription dynamicTableDescription) =>
        $"CREATE TABLE [{dynamicTableDescription.Schema}].[{dynamicTableDescription.Name}]";

    private static string CreateColumns(DynamicTableDescription desc) =>
        string.Join(",", desc.Columns.Select(CreateColumn));

    private static string CreateColumn(DynamicColumnDescription desc) => desc.PrimaryKey
        ? $"[{desc.Name}] {MapColumnType(desc.ColumnType, true)} NOT NULL PRIMARY KEY"
        : $"[{desc.Name}] {MapColumnType(desc.ColumnType, false)} NULL";

    private static string MapColumnType(ColumnType columnType, bool isPrimaryKey)
    {
        if (columnType == ColumnType.String)
        {
            if (isPrimaryKey)
            {
                return "[nvarchar](128)";
            }
            else
            {
                return "[nvarchar](4000)";
            }
        }
        else if (columnType == ColumnType.Guid)
        {
            return "[UNIQUEIDENTIFIER]";
        }
        else if (columnType == ColumnType.Int)
        {
            return "[int]";
        }
        else if (columnType == ColumnType.Geometry)
        {
            return "[geometry]";
        }
        else
        {
            throw new ArgumentException(
                $"Could not handle type {Enum.GetName(columnType)}");
        }
    }
}
