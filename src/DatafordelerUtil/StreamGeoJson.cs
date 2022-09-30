using System.Text.Json;
using System.Text.Json.Serialization;

namespace DatafordelerUtil;

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
    public Dictionary<string, dynamic?> Properties { get; }

    [JsonPropertyName("geometry")]
    public GeoJsonGeometry? Geometry { get; }

    [JsonConstructor]
    public GeoJsonFeature(
        string type,
        Dictionary<string, dynamic?> properties,
        GeoJsonGeometry geometry)
    {
        Type = type;
        Properties = properties;
        Geometry = geometry;
    }
}

internal static class StreamGeoJson
{
    public static IEnumerable<GeoJsonFeature> StreamFeaturesFile(string path)
    {
        using var sr = new StreamReader(path);
        string? line;
        while ((line = sr.ReadLine()) != null)
        {
            yield return JsonSerializer.Deserialize<GeoJsonFeature>(line) ??
                throw new InvalidOperationException(
                    $"Could not deserialize {nameof(GeoJsonFeature)}.");
        }
    }
}
