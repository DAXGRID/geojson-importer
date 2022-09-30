namespace DatafordelerUtil;

internal static class DynamicTableDescriptionFactory
{
    public static DynamicTableDescription Create(
        string? schema,
        string tableName,
        GeoJsonFeature geoJsonFeature)
    {
        return new DynamicTableDescription(
            schema: schema,
            name: tableName,
            columns: CreateDynamicColumnDescription(geoJsonFeature.Properties));
    }

    private static IEnumerable<DynamicColumnDescription> CreateDynamicColumnDescription(
        Dictionary<string, dynamic?> properties)
    {
        return properties.Select(x => new DynamicColumnDescription(x.Key, ColumnType.String));
    }
}
