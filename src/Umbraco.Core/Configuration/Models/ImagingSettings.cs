// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
///     Typed configuration options for imaging settings.
/// </summary>
[UmbracoOptions(Constants.Configuration.ConfigImaging)]
public class ImagingSettings
{
    /// <summary>
    ///     Gets or sets a value for imaging cache settings.
    /// </summary>
    public ImagingCacheSettings Cache { get; set; } = new();

    /// <summary>
    ///     Gets or sets a value for imaging resize settings.
    /// </summary>
    public ImagingResizeSettings Resize { get; set; } = new();
}
