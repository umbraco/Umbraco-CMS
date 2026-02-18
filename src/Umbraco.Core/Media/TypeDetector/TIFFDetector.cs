using System.Text;

namespace Umbraco.Cms.Core.Media.TypeDetector;

/// <summary>
///     Detects whether a file stream contains a TIFF (Tagged Image File Format) image by examining its header bytes.
/// </summary>
public class TIFFDetector
{
    /// <summary>
    ///     Determines whether the specified file stream contains a TIFF image.
    /// </summary>
    /// <param name="fileStream">The file stream to examine.</param>
    /// <returns><c>true</c> if the stream contains a TIFF image; otherwise, <c>false</c>.</returns>
    public static bool IsOfType(Stream fileStream)
    {
        var tiffHeader = GetFileHeader(fileStream);
        return (tiffHeader != null && tiffHeader == "MM\x00\x2a") || tiffHeader == "II\x2a\x00";
    }

    /// <summary>
    ///     Gets the TIFF file header string from the specified stream.
    /// </summary>
    /// <param name="fileStream">The file stream to read the header from.</param>
    /// <returns>The first 4 bytes of the file header as a string, or <c>null</c> if the header could not be read.</returns>
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
