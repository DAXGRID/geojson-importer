namespace DatafordelerUtil;

internal enum ColumnType
{
    String,
}

internal sealed record ColumnDescription
{
    public string Name { get; set; }
    public ColumnType ColumnType { get; }
    public uint? Length { get; }
    public bool Nullable { get; }

    public ColumnDescription(
        string name,
        ColumnType columnType)
    {
        Name = name;
        ColumnType = columnType;
    }

    public ColumnDescription(
        string name,
        ColumnType columnType,
        uint? length)
    {
        Name = name;
        ColumnType = columnType;
        Length = length;
    }

    public ColumnDescription(
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
    public IEnumerable<ColumnDescription> Properties { get; }

    public DynamicTableDescription(
        string name,
        IEnumerable<ColumnDescription> properties)
    {
        Name = name;
        Properties = properties;
    }
}

internal interface IDynamicSchemaBuilder
{
    string Create(DynamicTableDescription dynamicTableDescription);
}
