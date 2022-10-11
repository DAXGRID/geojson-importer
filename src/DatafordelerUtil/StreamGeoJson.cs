using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Converters;
using Newtonsoft.Json;

namespace DatafordelerUtil;

internal sealed record GeoJsonFeature
{
    [JsonProperty("type")]
    public string Type { get; }

    [JsonProperty("properties")]
    public Dictionary<string, string?> Properties { get; }

    [JsonProperty("geometry")]
    [JsonConverter(typeof(GeometryConverter))]
    public Geometry Geometry { get; }

    [JsonConstructor]
    public GeoJsonFeature(
        string type,
        Dictionary<string, string?> properties,
        Geometry geometry)
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

        return JsonConvert.DeserializeObject<GeoJsonFeature>(line) ??
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
                    .Select(x => JsonConvert.DeserializeObject<GeoJsonFeature>(x) ??
                            throw new InvalidOperationException(
                                $"Could not deserialize {nameof(GeoJsonFeature)}."));
                lines.Clear();
            }
        }

        yield return lines
            .AsParallel()
            .Select(x => JsonConvert.DeserializeObject<GeoJsonFeature>(x) ??
                    throw new InvalidOperationException(
                        $"Could not deserialize {nameof(GeoJsonFeature)}."));
    }
}
