namespace Umbraco.Cms.Core.Media.Exif;

/// <summary>
///     Represents a strip of compressed image data in a TIFF file.
/// </summary>
internal class TIFFStrip
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="TIFFStrip" /> class.
    /// </summary>
    /// <param name="data">The byte array to copy strip from.</param>
    /// <param name="offset">The offset to the beginning of strip.</param>
    /// <param name="length">The length of strip.</param>
    public TIFFStrip(byte[] data, uint offset, uint length)
    {
        Data = new byte[length];
        Array.Copy(data, offset, Data, 0, length);
    }

    /// <summary>
    ///     Compressed image data contained in this strip.
    /// </summary>
    public byte[] Data { get; }
}
