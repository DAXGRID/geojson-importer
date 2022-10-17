namespace DatafordelerUtil.Tests;

public sealed class StreamGeoJsonTest
{
    [Fact]
    [Trait("Category", "Integration")]
    public void Get_single_feature()
    {
        var filePath = TestUtil.AbsolutePath("Data/jordstykke.geojson");
        var feature = StreamGeoJson.FirstGeoJsonFeature(filePath);

        feature.Should().NotBeNull();
        feature.Properties["id"].Should().Be(4909206);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Stream_geojson_returns_geojson_features()
    {
        var filePath = TestUtil.AbsolutePath("Data/jordstykke.geojson");
        var stream = StreamGeoJson.StreamFeaturesFileAsync(filePath);

        var features = new List<GeoJsonFeature>();
        foreach (var feature in stream)
        {
            features.Add(feature);
        }

        features
            .Should()
            .HaveCount(1583)
            .And
            .AllSatisfy(x =>
            {
                x.Properties.Should().HaveCount(40);
                x.Geometry.Should().NotBeNull();
            });
    }
}
