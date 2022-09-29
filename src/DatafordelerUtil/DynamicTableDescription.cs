namespace DatafordelerUtil;

internal enum ColumnType
{
    String,
}

internal sealed record DynamicColumnDescription
{
    public string Name { get; set; }
    public ColumnType ColumnType { get; }
    public uint? Length { get; }
    public bool Nullable { get; }

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
        uint? length)
    {
        Name = name;
        ColumnType = columnType;
        Length = length;
    }

    public DynamicColumnDescription(
        string name,
        ColumnType columnType,
        uint? length,
        bool nullable)
    {
        Name = name;
        ColumnType = columnType;
        Length = length;
        Nullable = nullable;
    }
}

internal sealed record DynamicTableDescription
{
    public string Name { get; }
    public IEnumerable<DynamicColumnDescription> Columns { get; }

    public DynamicTableDescription(
        string name,
        IEnumerable<DynamicColumnDescription> columns)
    {
        Name = name;
        Columns = columns;
    }
}
