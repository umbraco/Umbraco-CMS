namespace Umbraco.Cms.Core.Media.Exif;

/// <summary>
///     Represents the JFIF version as a 16 bit unsigned integer. (EXIF Specification: SHORT)
/// </summary>
internal class JFIFVersion : ExifUShort
{
    public JFIFVersion(ExifTag tag, ushort value)
        : base(tag, value)
    {
    }

    /// <summary>
    ///     Gets the major version.
    /// </summary>
    public byte Major => (byte)(mValue >> 8);

    /// <summary>
    ///     Gets the minor version.
    /// </summary>
    public byte Minor => (byte)(mValue - ((mValue >> 8) * 256));

    public override string ToString() => string.Format("{0}.{1:00}", Major, Minor);
}

/// <summary>
///     Represents a JFIF thumbnail. (EXIF Specification: BYTE)
/// </summary>
internal class JFIFThumbnailProperty : ExifProperty
{
    protected JFIFThumbnail mValue;

    public JFIFThumbnailProperty(ExifTag tag, JFIFThumbnail value)
        : base(tag) =>
        mValue = value;

    public new JFIFThumbnail Value
    {
        get => mValue;
        set => mValue = value;
    }

    protected override object _Value
    {
        get => Value;
        set => Value = (JFIFThumbnail)value;
    }

    public override ExifInterOperability Interoperability
    {
        get
        {
            if (mValue.Format == JFIFThumbnail.ImageFormat.BMP24Bit)
            {
                return new ExifInterOperability(ExifTagFactory.GetTagID(mTag), 1, (uint)mValue.PixelData.Length, mValue.PixelData);
            }

            if (mValue.Format == JFIFThumbnail.ImageFormat.BMPPalette)
            {
                var data = new byte[mValue.Palette.Length + mValue.PixelData.Length];
                Array.Copy(mValue.Palette, data, mValue.Palette.Length);
                Array.Copy(mValue.PixelData, 0, data, mValue.Palette.Length, mValue.PixelData.Length);
                return new ExifInterOperability(ExifTagFactory.GetTagID(mTag), 1, (uint)data.Length, data);
            }

            if (mValue.Format == JFIFThumbnail.ImageFormat.JPEG)
            {
                return new ExifInterOperability(ExifTagFactory.GetTagID(mTag), 1, (uint)mValue.PixelData.Length, mValue.PixelData);
            }

            throw new InvalidOperationException("Unknown thumbnail type.");
        }
    }

    public override string ToString() => mValue.Format.ToString();
}
