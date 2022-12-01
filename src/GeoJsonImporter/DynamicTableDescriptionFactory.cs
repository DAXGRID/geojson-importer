namespace GeoJsonImporter;

internal static class DynamicTableDescriptionFactory
{
    public static DynamicTableDescription Create(
        string? schema,
        string tableName,
        string key,
        GeoJsonFeature geoJsonFeature,
        IReadOnlyDictionary<string, string> fieldNameMappings)
    {
        return new DynamicTableDescription(
            schema: schema,
            key: key,
            name: tableName,
            columns: CreateDynamicColumnDescription(
                geoJsonFeature, key, fieldNameMappings));
    }

    public static DynamicTableDescription Create(
        string? schema,
        string tableName,
        string key,
        IEnumerable<GeoJsonFeature> geoJsonFeatures,
        IReadOnlyDictionary<string, string> fieldNameMappings)
    {
        return new DynamicTableDescription(
            schema: schema,
            key: key,
            name: tableName,
            columns: CreateDynamicColumnDescription(
                geoJsonFeatures, key, fieldNameMappings));
    }

    private static IEnumerable<DynamicColumnDescription> CreateDynamicColumnDescription(
        IEnumerable<GeoJsonFeature> features,
        string primaryKeyFieldName,
        IReadOnlyDictionary<string, string> fieldMappings)
    {
        var properties = new Dictionary<string, DynamicColumnDescription>();

        foreach (var feature in features)
        {
            var columnDescriptions = CreateDynamicColumnDescription(
                feature,
                primaryKeyFieldName,
                fieldMappings);

            foreach (var columnDescription in columnDescriptions)
            {
                properties.TryAdd(columnDescription.Name, columnDescription);
            }
        }

        return properties.Select(x => x.Value);
    }

    private static IEnumerable<DynamicColumnDescription> CreateDynamicColumnDescription(
        GeoJsonFeature feature,
        string primaryKeyFieldName,
        IReadOnlyDictionary<string, string> fieldMappings)
    {
        var properties = feature.Properties
            .Select(
                x => new DynamicColumnDescription(
                    fieldMappings.TryGetValue(x.Key, out var mapping)
                    ? mapping ?? throw new InvalidOperationException("key mapping value cannot be null")
                    : x.Key,
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
