namespace Umbraco.Cms.Core.Media.Exif;

/// <summary>
///     Represents the units for the X and Y densities
///     for a JFIF file.
/// </summary>
internal enum JFIFDensityUnit : byte
{
    /// <summary>
    ///     No units, XDensity and YDensity specify the pixel aspect ratio.
    /// </summary>
    None = 0,

    /// <summary>
    ///     XDensity and YDensity are dots per inch.
    /// </summary>
    DotsPerInch = 1,

    /// <summary>
    ///     XDensity and YDensity are dots per cm.
    /// </summary>
    DotsPerCm = 2,
}

/// <summary>
///     Represents the JFIF extension.
/// </summary>
internal enum JFIFExtension : byte
{
    /// <summary>
    ///     Thumbnail coded using JPEG.
    /// </summary>
    ThumbnailJPEG = 0x10,

    /// <summary>
    ///     Thumbnail stored using a 256-Color RGB palette.
    /// </summary>
    ThumbnailPaletteRGB = 0x11,

    /// <summary>
    ///     Thumbnail stored using 3 bytes/pixel (24-bit) RGB values.
    /// </summary>
    Thumbnail24BitRGB = 0x13,
}
