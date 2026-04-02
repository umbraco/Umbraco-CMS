namespace Umbraco.Cms.Core.Media.TypeDetector;

/// <summary>
///     Base class for detecting rasterized image types by reading file headers.
/// </summary>
public abstract class RasterizedTypeDetector
{
    /// <summary>
    ///     Gets the file header bytes from the specified stream.
    /// </summary>
    /// <param name="fileStream">The file stream to read the header from.</param>
    /// <returns>The first 8 bytes of the file header, or <c>null</c> if the header could not be read.</returns>
    public static byte[]? GetFileHeader(Stream fileStream)
    {
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
