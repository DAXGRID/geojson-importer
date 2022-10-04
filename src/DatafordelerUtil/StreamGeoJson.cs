using System.Text.Json;
using System.Text.Json.Serialization;

namespace DatafordelerUtil;

public class DictionaryStringObjectJsonConverter
    : JsonConverter<Dictionary<string, object?>>
{
    public override Dictionary<string, object?> Read(
        ref Utf8JsonReader reader, Type? typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException(
                $"Invalid type {reader.TokenType}, only objects are supported");
        }

        var dictionary = new Dictionary<string, object?>();
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return dictionary;
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException("JsonTokenType was not PropertyName");
            }

            var propertyName = reader.GetString();

            if (string.IsNullOrWhiteSpace(propertyName))
            {
                throw new JsonException("Failed to get property name");
            }

            reader.Read();

            dictionary.Add(propertyName, ExtractValue(ref reader, options));
        }

        return dictionary;
    }

    public override void Write(
        Utf8JsonWriter writer, Dictionary<string, object?> value,
        JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, options);
    }

    private object? ExtractValue(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                if (reader.TryGetDateTime(out var date))
                {
                    return date;
                }
                return reader.GetString();
            case JsonTokenType.False:
                return false;
            case JsonTokenType.True:
                return true;
            case JsonTokenType.Null:
                return null;
            case JsonTokenType.Number:
                if (reader.TryGetInt64(out var result))
                {
                    return result;
                }
                return reader.GetDecimal();
            case JsonTokenType.StartObject:
                return Read(ref reader, null, options);
            case JsonTokenType.StartArray:
                var list = new List<object?>();
                while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                {
                    list.Add(ExtractValue(ref reader, options));
                }
                return list;
            default:
                throw new JsonException($"'{reader.TokenType}' is not supported");
        }
    }
}

internal sealed record GeoJsonGeometry
{
    [JsonPropertyName("type")]
    public string Type { get; }

    [JsonPropertyName("coordinates")]
    public dynamic Coordinates { get; }

    [JsonConstructor]
    public GeoJsonGeometry(string type, dynamic coordinates)
    {
        if (type == "Point")
        {
            Coordinates = ((JsonElement)coordinates).Deserialize<double[]>()
                ?? throw new ArgumentException(
                    $"Couldn't deserialize {coordinates}.", nameof(coordinates));
        }
        else if (type == "LineString")
        {
            Coordinates = ((JsonElement)coordinates).Deserialize<double[][]>()
                ?? throw new ArgumentException(
                    $"Couldn't deserialize {coordinates}.", nameof(coordinates));
        }
        else if (type == "Polygon")
        {
            Coordinates = ((JsonElement)coordinates).Deserialize<double[][][]>()
                ?? throw new ArgumentException(
                    $"Couldn't deserialize {coordinates}.", nameof(coordinates));
        }
        else
        {
            throw new ArgumentException($"Could not handle '{type}'.", nameof(type));
        }

        Type = type;
    }
}

internal sealed record GeoJsonFeature
{
    [JsonPropertyName("type")]
    public string Type { get; }

    [JsonPropertyName("properties")]
    [JsonConverter(typeof(DictionaryStringObjectJsonConverter))]
    public Dictionary<string, object?> Properties { get; }

    [JsonPropertyName("geometry")]
    public GeoJsonGeometry? Geometry { get; }

    [JsonConstructor]
    public GeoJsonFeature(
        string type,
        Dictionary<string, object?> properties,
        GeoJsonGeometry geometry)
    {
        Type = type;
        Properties = properties;
        Geometry = geometry;
    }
}

internal static class StreamGeoJson
{
    public static async Task<GeoJsonFeature> FirstGeoJsonFeatureAsync(string path)
    {
        using var sr = new StreamReader(path);
        var line = await sr.ReadLineAsync().ConfigureAwait(false);

        if (line is null)
        {
            throw new InvalidOperationException("Could not get the first line.");
        }

        return JsonSerializer.Deserialize<GeoJsonFeature>(line) ??
            throw new InvalidOperationException(
                $"Could not deserialize {nameof(GeoJsonFeature)}.");
    }

    public static async IAsyncEnumerable<IEnumerable<GeoJsonFeature>>
        StreamFeaturesFileAsync(string path, uint bulkCount)
    {
        using var sr = new StreamReader(path);
        string? line;

        var lines = new List<string>();
        while ((line = await sr.ReadLineAsync().ConfigureAwait(false)) != null)
        {
            lines.Add(line);
            if (lines.Count == bulkCount)
            {
                yield return lines
                    .AsParallel()
                    .Select(x => JsonSerializer.Deserialize<GeoJsonFeature>(x) ??
                            throw new InvalidOperationException(
                                $"Could not deserialize {nameof(GeoJsonFeature)}."));
                lines.Clear();
            }
        }

        yield return lines
            .AsParallel()
            .Select(x => JsonSerializer.Deserialize<GeoJsonFeature>(x) ??
                    throw new InvalidOperationException(
                        $"Could not deserialize {nameof(GeoJsonFeature)}."));
    }
}
