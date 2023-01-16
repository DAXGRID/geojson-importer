namespace GeoJsonImporter;

internal interface IDatafordelerDatabase
{
    Task Execute(string sql);
    Task CreateTable(DynamicTableDescription desc);
    Task DeleteTable(string name, string? schemaName = null);
    Task Merge(DynamicTableDescription target, DynamicTableDescription source);
    Task<bool> TableExists(string tableName, string? schemaName = null);
    Task BulkImportGeoJsonFeatures(
        DynamicTableDescription tableDescription,
        IReadOnlyList<GeoJsonFeature> features,
        IReadOnlyDictionary<string, string> fieldNameMappings);
    Task CreateSpatialIndex(string tableName, string? schemaName);
    Task<bool> IndexExists(string indexName, string tableName, string? schemaName);
}
