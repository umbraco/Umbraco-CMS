namespace Umbraco.Core.Media.Exif
{
    /// <summary>
    /// Represents a JFIF thumbnail.
    /// </summary>
    internal class JFIFThumbnail
    {
        #region Properties
        /// <summary>
        /// Gets the 256 color RGB palette.
        /// </summary>
        public byte[] Palette { get; private set; }
        /// <summary>
        /// Gets raw image data.
        /// </summary>
        public byte[] PixelData { get; private set; }
        /// <summary>
        /// Gets the image format.
        /// </summary>
        public ImageFormat Format { get; private set; }
        #endregion

        #region Public Enums
        public enum ImageFormat
        {
            JPEG,
            BMPPalette,
            BMP24Bit,
        }
        #endregion

        #region Constructors
        protected JFIFThumbnail()
        {
            Palette = new byte[0];
            PixelData = new byte[0];
        }

        public JFIFThumbnail(ImageFormat format, byte[] data)
            : this()
        {
            Format = format;
            PixelData = data;
        }

        public JFIFThumbnail(byte[] palette, byte[] data)
            : this()
        {
            Format = ImageFormat.BMPPalette;
            Palette = palette;
            PixelData = data;
        }
        #endregion
    }
}
