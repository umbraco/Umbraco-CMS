namespace Umbraco.Cms.Core.Media.Exif;

/// <summary>
///     Represents an image file directory.
/// </summary>
internal class ImageFileDirectory
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ImageFileDirectory" /> class.
    /// </summary>
    public ImageFileDirectory()
    {
        Fields = new List<ImageFileDirectoryEntry>();
        Strips = new List<TIFFStrip>();
    }

    /// <summary>
    ///     The fields contained in this IFD.
    /// </summary>
    public List<ImageFileDirectoryEntry> Fields { get; }

    /// <summary>
    ///     Offset to the next IFD.
    /// </summary>
    public uint NextIFDOffset { get; private set; }

    /// <summary>
    ///     Compressed image data.
    /// </summary>
    public List<TIFFStrip> Strips { get; }

    /// <summary>
    ///     Returns a <see cref="ImageFileDirectory" /> initialized from the given byte data.
    /// </summary>
    /// <param name="data">The data.</param>
    /// <param name="offset">The offset into <paramref name="data" />.</param>
    /// <param name="byteOrder">The byte order of <paramref name="data" />.</param>
    /// <returns>A <see cref="ImageFileDirectory" /> initialized from the given byte data.</returns>
    public static ImageFileDirectory FromBytes(byte[] data, uint offset, BitConverterEx.ByteOrder byteOrder)
    {
        var ifd = new ImageFileDirectory();
        var conv = new BitConverterEx(byteOrder, BitConverterEx.SystemByteOrder);

        var stripOffsets = new List<uint>();
        var stripLengths = new List<uint>();

        // Count
        var fieldcount = conv.ToUInt16(data, offset);

        // Read fields
        for (uint i = 0; i < fieldcount; i++)
        {
            var fieldoffset = offset + 2 + (12 * i);
            var field = ImageFileDirectoryEntry.FromBytes(data, fieldoffset, byteOrder);
            ifd.Fields.Add(field);

            // Read strip offsets
            if (field.Tag == 273)
            {
                var baselen = field.Data.Length / (int)field.Count;
                for (uint j = 0; j < field.Count; j++)
                {
                    var val = new byte[baselen];
                    Array.Copy(field.Data, j * baselen, val, 0, baselen);
                    var stripOffset = field.Type == 3 ? BitConverter.ToUInt16(val, 0) : BitConverter.ToUInt32(val, 0);
                    stripOffsets.Add(stripOffset);
                }
            }

            // Read strip lengths
            if (field.Tag == 279)
            {
                var baselen = field.Data.Length / (int)field.Count;
                for (uint j = 0; j < field.Count; j++)
                {
                    var val = new byte[baselen];
                    Array.Copy(field.Data, j * baselen, val, 0, baselen);
                    var stripLength = field.Type == 3 ? BitConverter.ToUInt16(val, 0) : BitConverter.ToUInt32(val, 0);
                    stripLengths.Add(stripLength);
                }
            }
        }

        // Save strips
        if (stripOffsets.Count != stripLengths.Count)
        {
            throw new NotValidTIFFileException();
        }

        for (var i = 0; i < stripOffsets.Count; i++)
        {
            ifd.Strips.Add(new TIFFStrip(data, stripOffsets[i], stripLengths[i]));
        }

        // Offset to next ifd
        ifd.NextIFDOffset = conv.ToUInt32(data, offset + 2 + (12 * fieldcount));

        return ifd;
    }
}
