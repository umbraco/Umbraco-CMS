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
    /// <summary>
    /// Gets or sets a value for the Hash-based Message Authentication Code (HMAC) secret key for request authentication.
    /// </summary>
    /// <remarks>
    /// Setting or updating this value will cause all existing generated URLs to become invalid and return a 400 Bad Request response code.
    /// The <see cref="ImagingResizeSettings.MaxWidth" />/<see cref="ImagingResizeSettings.MaxHeight" /> limits are still enforced even when this key is set.
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
