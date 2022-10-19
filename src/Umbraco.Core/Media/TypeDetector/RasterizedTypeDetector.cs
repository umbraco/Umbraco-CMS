namespace Umbraco.Cms.Core.Media.TypeDetector;

public abstract class RasterizedTypeDetector
{
    public static byte[]? GetFileHeader(Stream fileStream)
    {
        fileStream.Seek(0, SeekOrigin.Begin);
        var header = new byte[8];
        fileStream.Seek(0, SeekOrigin.Begin);

        // Invalid header
        if (fileStream.Read(header, 0, header.Length) != header.Length)
        {
            return null;
        }

        return header;
    }
}
