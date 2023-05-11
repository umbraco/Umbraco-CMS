// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
/// Typed configuration options for imaging settings.
/// </summary>
[UmbracoOptions(Constants.Configuration.ConfigImaging)]
public class ImagingSettings
{
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
}
