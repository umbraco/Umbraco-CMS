using System;

namespace Umbraco.Core.Media.Exif
{
    /// <summary>
    /// Represents the JFIF version as a 16 bit unsigned integer. (EXIF Specification: SHORT) 
    /// </summary>
    internal class JFIFVersion : ExifUShort
    {
        /// <summary>
        /// Gets the major version.
        /// </summary>
        public byte Major { get { return (byte)(mValue >> 8); } }
        /// <summary>
        /// Gets the minor version.
        /// </summary>
        public byte Minor { get { return (byte)(mValue - (mValue >> 8) * 256); } }

        public JFIFVersion(ExifTag tag, ushort value)
            : base(tag, value)
        {
            ;
        }

        public override string ToString()
        {
            return string.Format("{0}.{1:00}", Major, Minor);
        }
    }
    /// <summary>
    /// Represents a JFIF thumbnail. (EXIF Specification: BYTE) 
    /// </summary>
    internal class JFIFThumbnailProperty : ExifProperty
    {
        protected JFIFThumbnail mValue;
        protected override object _Value { get { return Value; } set { Value = (JFIFThumbnail)value; } }
        public new JFIFThumbnail Value { get { return mValue; } set { mValue = value; } }

        public override string ToString() { return mValue.Format.ToString(); }

        public JFIFThumbnailProperty(ExifTag tag, JFIFThumbnail value)
            : base(tag)
        {
            mValue = value;
        }

        public override ExifInterOperability Interoperability
        {
            get
            {
                if (mValue.Format == JFIFThumbnail.ImageFormat.BMP24Bit)
                    return new ExifInterOperability(ExifTagFactory.GetTagID(mTag), 1, (uint)mValue.PixelData.Length, mValue.PixelData);
                else if (mValue.Format == JFIFThumbnail.ImageFormat.BMPPalette)
                {
                    byte[] data = new byte[mValue.Palette.Length + mValue.PixelData.Length];
                    Array.Copy(mValue.Palette, data, mValue.Palette.Length);
                    Array.Copy(mValue.PixelData, 0, data, mValue.Palette.Length, mValue.PixelData.Length);
                    return new ExifInterOperability(ExifTagFactory.GetTagID(mTag), 1, (uint)data.Length, data);
                }
                else if (mValue.Format == JFIFThumbnail.ImageFormat.JPEG)
                    return new ExifInterOperability(ExifTagFactory.GetTagID(mTag), 1, (uint)mValue.PixelData.Length, mValue.PixelData);
                else
                    throw new InvalidOperationException("Unknown thumbnail type.");
            }
        }
    }
}
