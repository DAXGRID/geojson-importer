namespace DatafordelerUtil;

internal static class DynamicTableDescriptionFactory
{
    public static DynamicTableDescription Create(
        string? schema,
        string tableName,
        string key,
        GeoJsonFeature geoJsonFeature)
    {
        return new DynamicTableDescription(
            schema: schema,
            key: key,
            name: tableName,
            columns: CreateDynamicColumnDescription(geoJsonFeature, key));
    }

    private static IEnumerable<DynamicColumnDescription> CreateDynamicColumnDescription(
        GeoJsonFeature feature,
        string primaryKeyFieldName)
    {
        var properties = feature.Properties
            .Select(
                x => new DynamicColumnDescription(
                    x.Key,
                    GetColumnType(x.Value),
                    x.Key == primaryKeyFieldName));

        if (feature.Geometry is not null)
        {
            properties = properties
                .Append(new DynamicColumnDescription(
                            "coord",
                            ColumnType.Geometry,
                            false));
        }

        return properties;
    }

    private static ColumnType GetColumnType(object? o) => o switch
    {
        Guid => ColumnType.Guid,
        string => ColumnType.String,
        int => ColumnType.Int,
        long => ColumnType.Int,
        _ => ColumnType.String
    };
}
