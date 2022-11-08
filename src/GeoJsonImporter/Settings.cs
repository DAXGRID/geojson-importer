using System.Text.Json.Serialization;

namespace GeoJsonImporter;

internal sealed record ImportSetting
{
    [JsonPropertyName("tableName")]
    public string TableName { get; init; }

    [JsonPropertyName("keyFieldName")]
    public string KeyFieldName { get; init; }

    [JsonPropertyName("filePath")]
    public string FilePath { get; init; }

    [JsonPropertyName("schemaName")]
    public string? SchemaName { get; init; }

    [JsonPropertyName("fieldNameMappings")]
    public IReadOnlyDictionary<string, string> FieldNameMappings { get; init; }

    [JsonConstructor]
    public ImportSetting(
        string tableName,
        string keyFieldName,
        string filePath,
        string? schemaName,
        IReadOnlyDictionary<string, string>? fieldNameMappings)
    {
        if (string.IsNullOrWhiteSpace(tableName))
        {
            throw new ArgumentException(
                "Cannot be null, empty or whitespace.", nameof(tableName));
        }

        if (string.IsNullOrWhiteSpace(keyFieldName))
        {
            throw new ArgumentException(
                "Cannot be null, empty or whitespace.", nameof(keyFieldName));
        }

        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException(
                "Cannot be null, empty or whitespace.", nameof(filePath));
        }

        TableName = tableName;
        KeyFieldName = keyFieldName;
        FilePath = filePath;
        SchemaName = schemaName;
        FieldNameMappings = fieldNameMappings ?? new Dictionary<string, string>();
    }
}

internal sealed record Settings
{
    [JsonPropertyName("srid")]
    public int Srid { get; init; }

    [JsonPropertyName("spatialIndexStatement")]
    public string? SpartialIndexStatement { get; init; }

    [JsonPropertyName("connectionString")]
    public string ConnectionString { get; init; }

    [JsonPropertyName("imports")]
    public IEnumerable<ImportSetting> Imports { get; init; }

    [JsonConstructor]
    public Settings(
        int srid,
        string? spartialIndexStatement,
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
        Srid = srid;
        SpartialIndexStatement = spartialIndexStatement;
    }
}
