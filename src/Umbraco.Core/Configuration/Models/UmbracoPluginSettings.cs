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
    ///     to a build number or deployment id), a short hash of it forms the host part of each package's cache-bust
    ///     value — <c>&lt;version&gt;-&lt;hash&gt;</c>, appended as <c>umb__rnd</c> to importmap and extension assets —
    ///     so changing it forces a re-fetch even when the package's own <c>version</c> is unchanged. Only the hash
    ///     reaches the asset URLs, never the configured value itself. Empty by default (no effect).
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
