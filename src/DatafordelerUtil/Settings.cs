using System.Text.Json.Serialization;

namespace DatafordelerUtil;

internal sealed record ImportSetting
{
    [JsonPropertyName("layerName")]
    public string LayerName { get; }

    [JsonPropertyName("filePath")]
    public string FilePath { get; }

    public ImportSetting(string layerName, string filePath)
    {
        LayerName = layerName;
        FilePath = filePath;
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
