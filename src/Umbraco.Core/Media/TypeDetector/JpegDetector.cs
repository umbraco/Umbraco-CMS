namespace Umbraco.Cms.Core.Media.TypeDetector;

/// <summary>
///     Detects whether a file stream contains a JPEG image by examining its header bytes.
/// </summary>
public class JpegDetector : RasterizedTypeDetector
{
    /// <summary>
    ///     Determines whether the specified file stream contains a JPEG image.
    /// </summary>
    /// <param name="fileStream">The file stream to examine.</param>
    /// <returns><c>true</c> if the stream contains a JPEG image; otherwise, <c>false</c>.</returns>
    public static bool IsOfType(Stream fileStream)
    {
        var header = GetFileHeader(fileStream);
        return header != null && header[0] == 0xff && header[1] == 0xD8;
    }
}
