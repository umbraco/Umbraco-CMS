// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel;

namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
///     Typed configuration options for content imaging settings.
/// </summary>
public class ContentImagingSettings
{
    /// <summary>
    ///     The default set of accepted image file extensions.
    /// </summary>
    internal const string StaticImageFileTypes = "jpeg,jpg,gif,bmp,png,tiff,tif,webp";

    /// <summary>
    ///     The default set of native image formats that don't require conversion.
    ///     These are formats that image processors can handle directly without needing to convert to another format.
    /// </summary>
    internal const string StaticTrueImageFormats = "jpg,jpeg,png,gif,webp,bmp,tif,tiff";

    private static readonly ISet<ImagingAutoFillUploadField> _defaultImagingAutoFillUploadField = new HashSet<ImagingAutoFillUploadField>
    {
        new()
        {
            Alias = Constants.Conventions.Media.File,
            WidthFieldAlias = Constants.Conventions.Media.Width,
            HeightFieldAlias = Constants.Conventions.Media.Height,
            ExtensionFieldAlias = Constants.Conventions.Media.Extension,
            LengthFieldAlias = Constants.Conventions.Media.Bytes,
        },
    };

    /// <summary>
    ///     Gets or sets a value for the collection of accepted image file extensions.
    ///     This defines which file extensions are considered processable by the imaging system (includes formats like PDF that can be converted to images).
    ///     For configuration, see appsettings.json under Umbraco:CMS:Content:Imaging:ImageFileTypes.
    /// </summary>
    /// <remarks>
    ///     This setting determines which files can be processed by image manipulation middleware.
    ///     It includes both native image formats (JPG, PNG, etc.) and non-image formats that can be converted (PDF, EPS, etc.).
    ///     Compare with <see cref="TrueImageFormats"/> which defines only native image formats that don't require conversion.
    /// </remarks>
    [DefaultValue(StaticImageFileTypes)]
    public ISet<string> ImageFileTypes { get; set; } = new HashSet<string>(StaticImageFileTypes.Split(Constants.CharArrays.Comma));

    /// <summary>
    ///     Gets or sets a value for the collection of native image format extensions that don't require format conversion.
    ///     These are true image formats (JPG, PNG, GIF, WebP, etc.) that can be processed directly.
    ///     For configuration, see appsettings.json under Umbraco:CMS:Content:Imaging:TrueImageFormats.
    /// </summary>
    /// <remarks>
    ///     This setting determines which formats are considered "native" images that don't need conversion to another format.
    ///     Files with extensions NOT in this list (like PDF, EPS) will be automatically converted to a web-friendly format (typically WebP) when thumbnails are generated.
    ///     Compare with <see cref="ImageFileTypes"/> which includes all processable formats including those that require conversion.
    /// </remarks>
    [DefaultValue(StaticTrueImageFormats)]
    public ISet<string> TrueImageFormats { get; set; } = new HashSet<string>(StaticTrueImageFormats.Split(Constants.CharArrays.Comma));

    /// <summary>
    ///     Gets or sets the collection of media property mappings that are automatically populated with image metadata after a media file is uploaded.
    /// </summary>
    public ISet<ImagingAutoFillUploadField> AutoFillImageProperties { get; set; } = _defaultImagingAutoFillUploadField;
}
