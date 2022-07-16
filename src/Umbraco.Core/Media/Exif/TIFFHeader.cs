namespace Umbraco.Cms.Core.Media.Exif;

/// <summary>
///     Represents a TIFF Header.
/// </summary>
internal struct TIFFHeader
{
    /// <summary>
    ///     The byte order of the image file.
    /// </summary>
    public BitConverterEx.ByteOrder ByteOrder;

    /// <summary>
    ///     TIFF ID. This value should always be 42.
    /// </summary>
    public byte ID;

    /// <summary>
    ///     The offset to the first IFD section from the
    ///     start of the TIFF header.
    /// </summary>
    public uint IFDOffset;

    /// <summary>
    ///     The byte order of the TIFF header itself.
    /// </summary>
    public BitConverterEx.ByteOrder TIFFHeaderByteOrder;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TIFFHeader" /> struct.
    /// </summary>
    /// <param name="byteOrder">The byte order.</param>
    /// <param name="id">The TIFF ID. This value should always be 42.</param>
    /// <param name="ifdOffset">
    ///     The offset to the first IFD section from the
    ///     start of the TIFF header.
    /// </param>
    /// <param name="headerByteOrder">The byte order of the TIFF header itself.</param>
    public TIFFHeader(BitConverterEx.ByteOrder byteOrder, byte id, uint ifdOffset, BitConverterEx.ByteOrder headerByteOrder)
    {
        if (id != 42)
        {
            throw new NotValidTIFFHeader();
        }

        ByteOrder = byteOrder;
        ID = id;
        IFDOffset = ifdOffset;
        TIFFHeaderByteOrder = headerByteOrder;
    }

    /// <summary>
    ///     Returns a <see cref="TIFFHeader" /> initialized from the given byte data.
    /// </summary>
    /// <param name="data">The data.</param>
    /// <param name="offset">The offset into <paramref name="data" />.</param>
    /// <returns>A <see cref="TIFFHeader" /> initialized from the given byte data.</returns>
    public static TIFFHeader FromBytes(byte[] data, int offset)
    {
        var header = default(TIFFHeader);

        // TIFF header
        if (data[offset] == 0x49 && data[offset + 1] == 0x49)
        {
            header.ByteOrder = BitConverterEx.ByteOrder.LittleEndian;
        }
        else if (data[offset] == 0x4D && data[offset + 1] == 0x4D)
        {
            header.ByteOrder = BitConverterEx.ByteOrder.BigEndian;
        }
        else
        {
            throw new NotValidTIFFHeader();
        }

        // TIFF header may have a different byte order
        if (BitConverterEx.LittleEndian.ToUInt16(data, offset + 2) == 42)
        {
            header.TIFFHeaderByteOrder = BitConverterEx.ByteOrder.LittleEndian;
        }
        else if (BitConverterEx.BigEndian.ToUInt16(data, offset + 2) == 42)
        {
            header.TIFFHeaderByteOrder = BitConverterEx.ByteOrder.BigEndian;
        }
        else
        {
            throw new NotValidTIFFHeader();
        }

        header.ID = 42;

        // IFD offset
        header.IFDOffset =
            BitConverterEx.ToUInt32(data, offset + 4, header.TIFFHeaderByteOrder, BitConverterEx.SystemByteOrder);

        return header;
    }
}
