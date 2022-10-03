namespace DatafordelerUtil;

internal enum ColumnType
{
    String,
    Geometry
}

internal sealed record DynamicColumnDescription
{
    public string Name { get; set; }
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
    public string? Schema { get; }
    public string Name { get; }
    public IEnumerable<DynamicColumnDescription> Columns { get; }

    public DynamicTableDescription(
        string? schema,
        string name,
        IEnumerable<DynamicColumnDescription> columns)
    {
        Schema = schema;
        Name = name;
        Columns = columns;
    }
}
