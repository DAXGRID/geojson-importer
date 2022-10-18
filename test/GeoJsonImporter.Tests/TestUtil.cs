namespace GeoJsonImporter.Tests;

internal static class TestUtil
{
    public static string AbsolutePath(string path)
        => Path.IsPathRooted(path)
        ? path
        : Path.GetRelativePath(Directory.GetCurrentDirectory(), path);
}
