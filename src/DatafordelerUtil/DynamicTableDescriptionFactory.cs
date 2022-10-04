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
            columns: CreateDynamicColumnDescription(geoJsonFeature));
    }

    private static IEnumerable<DynamicColumnDescription> CreateDynamicColumnDescription(
        GeoJsonFeature feature)
    {
        var properties = feature.Properties
            .Select(x => new DynamicColumnDescription(x.Key, ColumnType.String));

        if (feature.Geometry is not null)
        {
            properties = properties
                .Append(new DynamicColumnDescription("coord", ColumnType.Geometry));
        }

        return properties;
    }
}
