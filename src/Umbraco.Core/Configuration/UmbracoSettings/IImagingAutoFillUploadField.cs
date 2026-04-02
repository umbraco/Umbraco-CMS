namespace Umbraco.Cms.Core.Configuration.UmbracoSettings;

/// <summary>
///     Defines the configuration for auto-filling image properties on media upload.
/// </summary>
public interface IImagingAutoFillUploadField
{
    /// <summary>
    ///     Gets the property alias that triggers the auto-fill behavior.
    /// </summary>
    /// <remarks>
    ///     Allow setting internally so we can create a default.
    /// </remarks>
    string Alias { get; }

    /// <summary>
    ///     Gets the alias of the property to store the image width.
    /// </summary>
    string WidthFieldAlias { get; }

    /// <summary>
    ///     Gets the alias of the property to store the image height.
    /// </summary>
    string HeightFieldAlias { get; }

    /// <summary>
    ///     Gets the alias of the property to store the file size in bytes.
    /// </summary>
    string LengthFieldAlias { get; }

    /// <summary>
    ///     Gets the alias of the property to store the file extension.
    /// </summary>
    string ExtensionFieldAlias { get; }
}
