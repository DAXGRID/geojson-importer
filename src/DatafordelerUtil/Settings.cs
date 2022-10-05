using System.Text.Json.Serialization;

namespace DatafordelerUtil;

internal sealed record ImportSetting
{
    [JsonPropertyName("tableName")]
    public string TableName { get; }

    [JsonPropertyName("keyFieldName")]
    public string KeyFieldName { get; }

    [JsonPropertyName("filePath")]
    public string FilePath { get; }

    [JsonPropertyName("schemaName")]
    public string? SchemaName { get; }

    public ImportSetting(
        string tableName,
        string keyFieldName,
        string filePath,
        string? schemaName)
    {
        if (string.IsNullOrWhiteSpace(tableName))
        {
            throw new ArgumentException("Cannot be null, empty or whitespace.", nameof(tableName));
        }

        if (string.IsNullOrWhiteSpace(keyFieldName))
        {
            throw new ArgumentException("Cannot be null, empty or whitespace.", nameof(keyFieldName));
        }

        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("Cannot be null, empty or whitespace.", nameof(filePath));
        }

        TableName = tableName;
        KeyFieldName = keyFieldName;
        FilePath = filePath;
        SchemaName = schemaName;
    }
}

internal sealed record Settings
{
    [JsonPropertyName("connectionString")]
    public string ConnectionString { get; }

    [JsonPropertyName("imports")]
    public IEnumerable<ImportSetting> Imports { get; }

    [JsonConstructor]
    public Settings(
        string connectionString,
        IEnumerable<ImportSetting> imports)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentNullException(
                nameof(connectionString),
                "cannot be null, empty or whitespace.");
        }

        ConnectionString = connectionString;
        Imports = imports;
    }
}
