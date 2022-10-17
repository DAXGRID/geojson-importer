namespace DatafordelerUtil;

internal enum ColumnType
{
    Guid,
    Int,
    String,
    Geometry
}

internal sealed record DynamicColumnDescription
{
    public string Name { get; init; }
    public ColumnType ColumnType { get; init; }
    public bool PrimaryKey { get; init; }

    public DynamicColumnDescription(
        string name,
        ColumnType columnType)
    {
        Name = name;
        ColumnType = columnType;
    }

    public DynamicColumnDescription(
        string name,
        ColumnType columnType,
        bool primaryKey)
    {
        Name = name;
        ColumnType = columnType;
        PrimaryKey = primaryKey;
    }
}

internal sealed record DynamicTableDescription
{
    public string Name { get; init; }
    public string Key { get; init; }
    public string? Schema { get; init; }
    public IEnumerable<DynamicColumnDescription> Columns { get; init; }

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
