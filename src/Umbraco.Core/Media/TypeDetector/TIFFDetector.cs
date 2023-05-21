using System.Text;

namespace Umbraco.Cms.Core.Media.TypeDetector;

public class TIFFDetector
{
    public static bool IsOfType(Stream fileStream)
    {
        var tiffHeader = GetFileHeader(fileStream);
        return (tiffHeader != null && tiffHeader == "MM\x00\x2a") || tiffHeader == "II\x2a\x00";
    }

    public static string? GetFileHeader(Stream fileStream)
    {
        var header = RasterizedTypeDetector.GetFileHeader(fileStream);
        if (header == null)
        {
            return null;
        }

        var tiffHeader = Encoding.ASCII.GetString(header, 0, 4);
        return tiffHeader;
    }
}
