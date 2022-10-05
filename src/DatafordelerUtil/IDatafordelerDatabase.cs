namespace DatafordelerUtil;

internal interface IDatafordelerDatabase
{
    Task Execute(string sql);
    Task CreateTable(DynamicTableDescription desc);
    Task DeleteTable(string name, string? schemaName = null);
    Task Merge(DynamicTableDescription target, DynamicTableDescription source);
    Task<bool> TableExists(string tableName, string? schemaName = null);
    Task BulkImportGeoJsonFeatures(string tableName, IEnumerable<GeoJsonFeature> features);
}
