namespace Umbraco.Cms.Core.Media.Exif;

/// <summary>
///     Represents the format of the <see cref="ImageFile" />.
/// </summary>
internal enum ImageFileFormat
{
    /// <summary>
    ///     The file is not recognized.
    /// </summary>
    Unknown,

    /// <summary>
    ///     The file is a JPEG/Exif or JPEG/JFIF file.
    /// </summary>
    JPEG,

    /// <summary>
    ///     The file is a TIFF File.
    /// </summary>
    TIFF,

    /// <summary>
    ///     The file is a SVG File.
    /// </summary>
    SVG,
}
