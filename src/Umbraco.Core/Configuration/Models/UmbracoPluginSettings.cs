// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel;

namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
///     Typed configuration options for the plugins.
/// </summary>
[UmbracoOptions(Constants.Configuration.ConfigPlugins)]
public class UmbracoPluginSettings
{
    /// <summary>
    ///     Gets or sets an optional host-controlled cache-buster for package <c>/App_Plugins</c> assets. When set (e.g.
    ///     to a build number or deployment id), it is appended as <c>umb__rnd</c> to every package's assets — importmap
    ///     and extensions — forcing a re-fetch regardless of each package's own <c>version</c>. Empty by default (no effect).
    /// </summary>
    [DefaultValue("")]
    public string Cachebuster { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the allowed file extensions (including the period ".") that should be accessible from the browser.
    /// </summary>
    /// WB-TODO
    public ISet<string> BrowsableFileExtensions { get; set; } = new HashSet<string>(new[]
    {
        ".html", // markup
        ".css", // styles
        ".js", // scripts
        ".jpg", ".jpeg", ".gif", ".png", ".svg", // images
        ".eot", ".ttf", ".woff", ".woff2", // fonts
        ".xml", ".json", ".config", // configurations
        ".lic", // license
        ".map", // js map files
    });
}
