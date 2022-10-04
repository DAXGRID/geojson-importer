namespace DatafordelerUtil.Tests;

public sealed class StreamGeoJsonTest
{

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Get_single_feature()
    {
        var filePath = TestUtil.AbsolutePath("Data/jordstykke.geojson");
        var feature = await StreamGeoJson.FirstGeoJsonFeatureAsync(filePath);

        feature.Should().NotBeNull();
        feature.Properties["id"].Should().Be(4909206);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Stream_geojson_returns_geojson_features()
    {
        var filePath = TestUtil.AbsolutePath("Data/jordstykke.geojson");
        var stream = StreamGeoJson.StreamFeaturesFileAsync(filePath, 5000);

        var features = new List<GeoJsonFeature>();
        await foreach (var feature in stream)
        {
            features.AddRange(feature);
        }

        features
            .Should()
            .HaveCount(1583)
            .And
            .AllSatisfy(x =>
            {
                x.Properties.Should().HaveCount(40);
                x.Geometry?.Type.Should().NotBeEmpty();
                ((double[][][]?)x.Geometry?.Coordinates)?.Should().NotBeEmpty();
            });
    }
}
