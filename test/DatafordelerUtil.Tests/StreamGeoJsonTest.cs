namespace DatafordelerUtil.Tests;

public sealed class StreamGeoJsonTest
{
    [Fact]
    [Trait("Category", "Integration")]
    public void Stream_geojson_returns_geojson_features()
    {
        var filePath = TestUtil.AbsolutePath("Data/jordstykke.geojson");
        var features = StreamGeoJson.StreamFeaturesFile(filePath).ToList();

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
