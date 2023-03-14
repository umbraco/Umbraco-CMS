// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel;

namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
///     Typed configuration options for image resize settings.
/// </summary>
public class ImagingResizeSettings
{
    internal const int StaticMaxWidth = 5000;
    internal const int StaticMaxHeight = 5000;

    /// <summary>
    ///     Gets or sets a value for the maximim resize width.
    /// </summary>
    [DefaultValue(StaticMaxWidth)]
    public int MaxWidth { get; set; } = StaticMaxWidth;

    /// <summary>
    ///     Gets or sets a value for the maximim resize height.
    /// </summary>
    [DefaultValue(StaticMaxHeight)]
    public int MaxHeight { get; set; } = StaticMaxHeight;
}
