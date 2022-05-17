namespace Umbraco.Cms.Core.Media.TypeDetector;

public class JpegDetector : RasterizedTypeDetector
{
    public static bool IsOfType(Stream fileStream)
    {
        var header = GetFileHeader(fileStream);
        return header != null && header[0] == 0xff && header[1] == 0xD8;
    }
}
