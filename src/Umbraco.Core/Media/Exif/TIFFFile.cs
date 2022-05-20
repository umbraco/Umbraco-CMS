using System.Text;

namespace Umbraco.Cms.Core.Media.Exif;

/// <summary>
///     Represents the binary view of a TIFF file.
/// </summary>
internal class TIFFFile : ImageFile
{
    #region Constructor

    /// <summary>
    ///     Initializes a new instance of the <see cref="JPEGFile" /> class from the
    ///     specified data stream.
    /// </summary>
    /// <param name="stream">A <see cref="Sytem.IO.Stream" /> that contains image data.</param>
    /// <param name="encoding">The encoding to be used for text metadata when the source encoding is unknown.</param>
    protected internal TIFFFile(Stream stream, Encoding encoding)
    {
        Format = ImageFileFormat.TIFF;
        IFDs = new List<ImageFileDirectory>();
        Encoding = encoding;

        // Read the entire stream
        var data = Utility.GetStreamBytes(stream);

        // Read the TIFF header
        TIFFHeader = TIFFHeader.FromBytes(data, 0);
        var nextIFDOffset = TIFFHeader.IFDOffset;
        if (nextIFDOffset == 0)
        {
            throw new NotValidTIFFileException("The first IFD offset is zero.");
        }

        // Read IFDs in order
        while (nextIFDOffset != 0)
        {
            var ifd = ImageFileDirectory.FromBytes(data, nextIFDOffset, TIFFHeader.ByteOrder);
            nextIFDOffset = ifd.NextIFDOffset;
            IFDs.Add(ifd);
        }

        // Process IFDs
        // TODO: Add support for multiple frames
        foreach (ImageFileDirectoryEntry field in IFDs[0].Fields)
        {
            Properties.Add(ExifPropertyFactory.Get(field.Tag, field.Type, field.Count, field.Data, BitConverterEx.SystemByteOrder, IFD.Zeroth, Encoding));
        }
    }

    #endregion

    #region Properties

    /// <summary>
    ///     Gets the TIFF header.
    /// </summary>
    public TIFFHeader TIFFHeader { get; }

    #endregion

    #region Instance Methods

    /// <summary>
    ///     Saves the <see cref="ImageFile" /> to the given stream.
    /// </summary>
    /// <param name="stream">The data stream used to save the image.</param>
    public override void Save(Stream stream)
    {
        BitConverterEx conv = BitConverterEx.SystemEndian;

        // Write TIFF header
        uint ifdoffset = 8;

        // Byte order
        stream.Write(
            BitConverterEx.SystemByteOrder == BitConverterEx.ByteOrder.LittleEndian
                ? new byte[] { 0x49, 0x49 }
                : new byte[] { 0x4D, 0x4D },
            0,
            2);

        // TIFF ID
        stream.Write(conv.GetBytes((ushort)42), 0, 2);

        // Offset to 0th IFD, will be corrected below
        stream.Write(conv.GetBytes(ifdoffset), 0, 4);

        // Write IFD sections
        for (var i = 0; i < IFDs.Count; i++)
        {
            ImageFileDirectory ifd = IFDs[i];

            // Save the location of IFD offset
            var ifdLocation = stream.Position - 4;

            // Write strips first
            var stripOffsets = new byte[4 * ifd.Strips.Count];
            var stripLengths = new byte[4 * ifd.Strips.Count];
            var stripOffset = ifdoffset;
            for (var j = 0; j < ifd.Strips.Count; j++)
            {
                var stripData = ifd.Strips[j].Data;
                var oBytes = BitConverter.GetBytes(stripOffset);
                var lBytes = BitConverter.GetBytes((uint)stripData.Length);
                Array.Copy(oBytes, 0, stripOffsets, 4 * j, 4);
                Array.Copy(lBytes, 0, stripLengths, 4 * j, 4);
                stream.Write(stripData, 0, stripData.Length);
                stripOffset += (uint)stripData.Length;
            }

            // Remove old strip tags
            for (var j = ifd.Fields.Count - 1; j > 0; j--)
            {
                var tag = ifd.Fields[j].Tag;
                if (tag == 273 || tag == 279)
                {
                    ifd.Fields.RemoveAt(j);
                }
            }

            // Write new strip tags
            ifd.Fields.Add(new ImageFileDirectoryEntry(273, 4, (uint)ifd.Strips.Count, stripOffsets));
            ifd.Fields.Add(new ImageFileDirectoryEntry(279, 4, (uint)ifd.Strips.Count, stripLengths));

            // Write fields after strips
            ifdoffset = stripOffset;

            // Correct IFD offset
            var currentLocation = stream.Position;
            stream.Seek(ifdLocation, SeekOrigin.Begin);
            stream.Write(conv.GetBytes(ifdoffset), 0, 4);
            stream.Seek(currentLocation, SeekOrigin.Begin);

            // Offset to field data
            var dataOffset = ifdoffset + 2 + ((uint)ifd.Fields.Count * 12) + 4;

            // Field count
            stream.Write(conv.GetBytes((ushort)ifd.Fields.Count), 0, 2);

            // Fields
            foreach (ImageFileDirectoryEntry field in ifd.Fields)
            {
                // Tag
                stream.Write(conv.GetBytes(field.Tag), 0, 2);

                // Type
                stream.Write(conv.GetBytes(field.Type), 0, 2);

                // Count
                stream.Write(conv.GetBytes(field.Count), 0, 4);

                // Field data
                var data = field.Data;
                if (data.Length <= 4)
                {
                    stream.Write(data, 0, data.Length);
                    for (var j = data.Length; j < 4; j++)
                    {
                        stream.WriteByte(0);
                    }
                }
                else
                {
                    stream.Write(conv.GetBytes(dataOffset), 0, 4);
                    var currentOffset = stream.Position;
                    stream.Seek(dataOffset, SeekOrigin.Begin);
                    stream.Write(data, 0, data.Length);
                    dataOffset += (uint)data.Length;
                    stream.Seek(currentOffset, SeekOrigin.Begin);
                }
            }

            // Offset to next IFD
            ifdoffset = dataOffset;
            stream.Write(conv.GetBytes(i == IFDs.Count - 1 ? 0 : ifdoffset), 0, 4);
        }
    }

    /// <summary>
    ///     Gets the image file directories.
    /// </summary>
    public List<ImageFileDirectory> IFDs { get; }

    #endregion
}
