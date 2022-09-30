using System.Text.Json.Serialization;

namespace DatafordelerUtil;

internal sealed record ImportSetting
{
    [JsonPropertyName("tableName")]
    public string TableName { get; }

    [JsonPropertyName("filePath")]
    public string FilePath { get; }

    [JsonPropertyName("schemaName")]
    public string? SchemaName { get; }

    public ImportSetting(
        string layerName,
        string filePath,
        string? schemaName)
    {
        TableName = layerName;
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
