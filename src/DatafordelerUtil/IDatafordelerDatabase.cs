namespace DatafordelerUtil;

internal interface IDatafordelerDatabase
{
    Task Execute(string sql);
    Task CreateTable(DynamicTableDescription desc);
    Task<bool> TableExists(string tableName, string? schemaName);
    Task BulkImportGeoJsonFeatures(string tableName, IEnumerable<GeoJsonFeature> features);
}
