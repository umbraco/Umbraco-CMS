// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel;

namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
///     Typed configuration options for image cache settings.
/// </summary>
public class ImagingCacheSettings
{
    /// <summary>
    ///     The default browser cache maximum age.
    /// </summary>
    internal const string StaticBrowserMaxAge = "7.00:00:00";

    /// <summary>
    ///     The default image cache maximum age.
    /// </summary>
    internal const string StaticCacheMaxAge = "365.00:00:00";

    /// <summary>
    ///     The default image cache hash length.
    /// </summary>
    internal const int StaticCacheHashLength = 12;

    /// <summary>
    ///     The default image cache folder depth.
    /// </summary>
    internal const int StaticCacheFolderDepth = 8;

    /// <summary>
    ///     The default image cache folder path.
    /// </summary>
    internal const string StaticCacheFolder = Constants.SystemDirectories.TempData + "/MediaCache";

    /// <summary>
    ///     Gets or sets a value for the browser image cache maximum age.
    /// </summary>
    [DefaultValue(StaticBrowserMaxAge)]
    public TimeSpan BrowserMaxAge { get; set; } = TimeSpan.Parse(StaticBrowserMaxAge);

    /// <summary>
    ///     Gets or sets a value for the image cache maximum age.
    /// </summary>
    [DefaultValue(StaticCacheMaxAge)]
    public TimeSpan CacheMaxAge { get; set; } = TimeSpan.Parse(StaticCacheMaxAge);

    /// <summary>
    ///     Gets or sets a value for the image cache hash length.
    /// </summary>
    [DefaultValue(StaticCacheHashLength)]
    public uint CacheHashLength { get; set; } = StaticCacheHashLength;

    /// <summary>
    ///     Gets or sets a value for the image cache folder depth.
    /// </summary>
    [DefaultValue(StaticCacheFolderDepth)]
    public uint CacheFolderDepth { get; set; } = StaticCacheFolderDepth;

    /// <summary>
    ///     Gets or sets a value for the image cache folder.
    /// </summary>
    [DefaultValue(StaticCacheFolder)]
    public string CacheFolder { get; set; } = StaticCacheFolder;
}
