// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel;

namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
/// Typed configuration options for imaging settings.
/// </summary>
[UmbracoOptions(Constants.Configuration.ConfigImaging)]
public class ImagingSettings
{
    private const bool StaticEnableMediaRecycleBinProtection = false;

    /// <summary>
    /// Gets or sets a value for the Hash-based Message Authentication Code (HMAC) secret key for request authentication.
    /// </summary>
    /// <remarks>
    /// Setting or updating this value will cause all existing generated URLs to become invalid and return a 400 Bad Request response code.
    /// When set, the maximum resize settings are not used/validated anymore, because you can only request URLs with a valid HMAC token anyway.
    /// </remarks>
    public byte[] HMACSecretKey { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Gets or sets a value for imaging cache settings.
    /// </summary>
    public ImagingCacheSettings Cache { get; set; } = new();

    /// <summary>
    /// Gets or sets a value for imaging resize settings.
    /// </summary>
    public ImagingResizeSettings Resize { get; set; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether to enable or disable the recycle bin protection for media.
    /// </summary>
    /// <remarks>
    /// When set to true, this will:
    ///  - Rename media moved to the recycle bin to have a .deleted suffice (e.g. image.jpg will be renamed to image.deleted.jpg).
    ///  - On restore, the media file will be renamed back to its original name.
    ///  - A middleware component will be enabled to prevent access to media files in the recycle bin unless the user is authenticated with access to the media section.
    /// </remarks>
    [DefaultValue(StaticEnableMediaRecycleBinProtection)]
    public bool EnableMediaRecycleBinProtection { get; set; } = StaticEnableMediaRecycleBinProtection;
}
