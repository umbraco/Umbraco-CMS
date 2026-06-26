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
    ///     Gets or sets an optional host-controlled cache-buster for package <c>/App_Plugins</c> assets. When set — for
    ///     example to a build number, git SHA or deployment id on each release — it is supplied to the backoffice and
    ///     appended (as <c>umb__rnd</c>) to every package's clean <c>/App_Plugins</c> JavaScript URLs, in both the
    ///     importmap and the registered extensions, forcing all package assets to be re-fetched regardless of whether
    ///     each package bumped its own <c>version</c>. Empty by default, in which case it has no effect.
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
