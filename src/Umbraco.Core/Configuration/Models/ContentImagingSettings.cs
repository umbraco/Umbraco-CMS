// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel;

namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
///     Typed configuration options for content imaging settings.
/// </summary>
public class ContentImagingSettings
{
    internal const string StaticImageFileTypes = "jpeg,jpg,gif,bmp,png,tiff,tif,webp";

    private static readonly ISet<ImagingAutoFillUploadField> DefaultImagingAutoFillUploadField = new HashSet<ImagingAutoFillUploadField>
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
    /// </summary>
    [DefaultValue(StaticImageFileTypes)]
    public ISet<string> ImageFileTypes { get; set; } = new HashSet<string>(StaticImageFileTypes.Split(Constants.CharArrays.Comma));

    /// <summary>
    ///     Gets or sets a value for the imaging autofill following media file upload fields.
    /// </summary>
    /// <value>
    public ISet<ImagingAutoFillUploadField> AutoFillImageProperties { get; set; } = DefaultImagingAutoFillUploadField;
}
