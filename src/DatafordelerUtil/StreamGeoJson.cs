using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Converters;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

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
    public static GeoJsonFeature FirstGeoJsonFeature(string path)
    {
        var regex = new Regex(@"features");

        using var stream = new FileStream(path, FileMode.Open);
        using var streamReader = new StreamReader(stream);
        using var jsonReader = new JsonTextReader(streamReader);

        var features = jsonReader
            .SelectTokensWithRegex<GeoJsonFeature[]>(regex).FirstOrDefault() ??
            throw new InvalidOperationException(
                "Could not get features.");

        return features.FirstOrDefault() ??
            throw new InvalidOperationException("Could not get first feature.");
    }

    public static IEnumerable<GeoJsonFeature> StreamFeaturesFile(string path)
    {
        var regex = new Regex(@"features");

        using var stream = new FileStream(path, FileMode.Open);
        using var streamReader = new StreamReader(stream);
        using var jsonReader = new JsonTextReader(streamReader);

        var features = jsonReader
            .SelectTokensWithRegex<GeoJsonFeature[]>(regex)
            .FirstOrDefault();

        if (features is null)
        {
            throw new InvalidOperationException(
                "Could not find 'features' token in stream.");
        }

        foreach (var feature in features)
        {
            yield return feature;
        }
    }
}
