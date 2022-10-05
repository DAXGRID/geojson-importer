namespace DatafordelerUtil;

internal enum ColumnType
{
    String,
    Geometry
}

internal sealed record DynamicColumnDescription
{
    public string Name { get; }
    public ColumnType ColumnType { get; }

    public DynamicColumnDescription(
        string name,
        ColumnType columnType)
    {
        Name = name;
        ColumnType = columnType;
    }
}

internal sealed record DynamicTableDescription
{
    public string Name { get; }
    public string Key { get; }
    public string? Schema { get; }
    public IEnumerable<DynamicColumnDescription> Columns { get; }

    public DynamicTableDescription(
        string name,
        string key,
        IEnumerable<DynamicColumnDescription> columns,
        string? schema)
    {
        Name = name;
        Key = key;
        Columns = columns;
        Schema = schema;
    }
}
