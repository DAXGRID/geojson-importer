namespace DatafordelerUtil.Tests;

public sealed class StreamGeoJsonTest
{
    [Fact]
    public void Stream_geojson_returns_geojson_features()
    {
        using var fileStream = new FileStream(
            TestUtil.AbsolutePath("Data/jordstykke.geojson"),
            FileMode.Open);

        var features = StreamGeoJson.StreamFeatures(fileStream).ToList();

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
