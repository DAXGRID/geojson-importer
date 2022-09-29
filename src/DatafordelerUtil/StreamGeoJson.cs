using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace DatafordelerUtil;

internal sealed record GeoJsonGeometry
{
    public string Type { get; }
    public dynamic Coordinates { get; }

    public GeoJsonGeometry(string type, JArray coordinates)
    {
        if (type == "Point")
        {
            Coordinates = coordinates.ToObject<double[]>()
                ?? throw new ArgumentException("Could not convert Point.");
        }
        else if (type == "LineString")
        {
            Coordinates = coordinates.ToObject<double[][]>()
                ?? throw new ArgumentException("Could not convert LineString.");
        }
        else if (type == "Polygon")
        {
            Coordinates = coordinates.ToObject<double[][][]>()
                ?? throw new ArgumentException("Could not convert Polygon.");
        }
        else
        {
            throw new ArgumentException($"Could not handle type {type}.");
        }

        Type = type;
    }
}

internal sealed record GeoJsonFeature
{
    public string Type { get; }
    public Dictionary<string, string?> Properties { get; }
    public GeoJsonGeometry? Geometry { get; }

    public GeoJsonFeature(
        string type,
        Dictionary<string, string?> properties,
        GeoJsonGeometry geometry)
    {
        Type = type;
        Properties = properties;
        Geometry = geometry;
    }
}

internal static class StreamGeoJson
{
    public static IEnumerable<GeoJsonFeature> StreamFeatures(Stream stream)
    {
        var regex = new Regex(@"features");

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
