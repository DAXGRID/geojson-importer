using DatafordelerUtil.Converter;
using NetTopologySuite.Geometries;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DatafordelerUtil;

internal sealed record GeoJsonGeometry
{
    [JsonPropertyName("type")]
    public string Type { get; }

    [JsonPropertyName("coordinates")]
    public dynamic Coordinates { get; }

    public Geometry AsGeometry() => Type switch
    {
        "Point" => new Point(((double[])Coordinates)[0], ((double[])Coordinates)[1]),
        "LineString" => new LineString(
            ((double[][])Coordinates)
            .Select(x => new Coordinate(x[0], x[1]))
            .ToArray()),
        "Polygon" => new Polygon(
            new LinearRing(
                ((double[][][])Coordinates)
                .SelectMany(x => x.Select(y => new Coordinate(y[0], y[1])))
                .ToArray())),
        _ => throw new InvalidOperationException(
            $"Cannot handle geometry of type {Type}.")
    };

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
