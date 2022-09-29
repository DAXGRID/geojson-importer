namespace DatafordelerUtil;

internal interface IDatafordelerDatabase
{
    Task Execute(string sql);
    Task CreateTable(DynamicTableDescription desc);
    Task<bool> TableExists(string tableName, string? schemaName);
}
