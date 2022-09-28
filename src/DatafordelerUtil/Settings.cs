using System.Text.Json.Serialization;

namespace DatafordelerUtil;

internal record Settings
{
    [JsonPropertyName("connectionString")]
    public string ConnectionString { get; }

    [JsonConstructor]
    public Settings(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentNullException(
                nameof(connectionString),
                "cannot be null, empty or whitespace.");
        }

        ConnectionString = connectionString;
    }
}
