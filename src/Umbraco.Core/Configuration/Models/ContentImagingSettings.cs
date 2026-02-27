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
    ///     The default set of native image file extensions that don't require format conversion.
    /// </summary>
    internal const string StaticImageFileTypes = "jpeg,jpg,gif,bmp,png,tiff,tif,webp";

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
    ///     Gets or sets a value for the collection of native image format extensions that don't require format conversion.
    ///     These are true image formats (JPG, PNG, GIF, WebP, etc.) that can be processed directly without conversion.
    ///     For configuration, see appsettings.json under Umbraco:CMS:Content:Imaging:ImageFileTypes.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     This setting is used in multiple ways throughout Umbraco:
    ///     </para>
    ///     <list type="number">
    ///         <item>
    ///             <term>Thumbnail Format Determination (Factory Layer)</term>
    ///             <description>
    ///             When generating image thumbnails via the Management API, the resize factory checks if the source file extension
    ///             is in this list. If the extension is NOT found (e.g., PDF, EPS), the system automatically converts the output to
    ///             WebP format for browser compatibility. Native image formats keep their original format unless explicitly overridden.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <term>Backoffice UI Configuration (Temporary File API)</term>
    ///             <description>
    ///             This list is exposed to the Umbraco backoffice via the temporary file configuration endpoint
    ///             (/umbraco/management/api/v1/temporary-file/configuration). The frontend uses this to understand which file extensions
    ///             represent native images vs. other processable file types. This helps the UI make informed decisions about how to
    ///             handle different media file types.
    ///             </description>
    ///         </item>
    ///     </list>
    ///     <para>
    ///     Configuration example (appsettings.json):
    ///     </para>
    ///     <code>
    ///     {
    ///       "Umbraco": {
    ///         "CMS": {
    ///           "Content": {
    ///             "Imaging": {
    ///               "ImageFileTypes": ["jpeg", "jpg", "gif", "bmp", "png", "tiff", "tif", "webp"]
    ///             }
    ///           }
    ///         }
    ///       }
    ///     }
    ///     </code>
    ///     <para>
    ///     <strong>Note:</strong> This setting defines which formats are "native images" and is separate from what ImageSharp can process.
    ///     ImageSharp may support additional formats through plugins (e.g., PDF via community packages), but those would not be in this
    ///     list since they require conversion to a standard image format.
    ///     </para>
    /// </remarks>
    [DefaultValue(StaticImageFileTypes)]
    public ISet<string> ImageFileTypes { get; set; } = new HashSet<string>(StaticImageFileTypes.Split(Constants.CharArrays.Comma));

    /// <summary>
    ///     Gets or sets the collection of media property mappings that are automatically populated with image metadata after a media file is uploaded.
    /// </summary>
    public ISet<ImagingAutoFillUploadField> AutoFillImageProperties { get; set; } = _defaultImagingAutoFillUploadField;
}
