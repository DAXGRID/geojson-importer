namespace DatafordelerUtil.SqlServer;

internal sealed class SqlServerDynamicSchemaBuilder : IDynamicSchemaBuilder
{
    public string Create(DynamicTableDescription desc)
    {
        return $"{CreateTable(desc)}({CreateColumns(desc)})";
    }

    private static string CreateColumns(DynamicTableDescription desc) =>
        string.Join(",", desc.Properties.Select(CreateColumn));

    private static string CreateColumn(ColumnDescription desc)
    {
        var column = $"[{desc.Name}] [{ConvertColumnType(desc.ColumnType)}]";

        if (desc.Length is not null)
        {
            column += $"({desc.Length})";
        }

        column += desc.Nullable ? " NULL" : " NOT NULL";

        return column;
    }

    private static string ConvertColumnType(ColumnType columnType) => columnType switch
    {
        ColumnType.String => "varchar",
        _ => throw new ArgumentException(
            $"Could not convert {Enum.GetName(columnType)}.",
            nameof(columnType))
    };

    private static string CreateTable(DynamicTableDescription dynamicTableDescription) =>
        $"CREATE TABLE [dbo].[{dynamicTableDescription.Name}]";
}
