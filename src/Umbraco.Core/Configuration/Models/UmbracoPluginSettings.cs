// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;

namespace Umbraco.Core.Configuration.Models
{
    /// <summary>
    /// Typed configuration options for the plugins.
    /// </summary>
    public class UmbracoPluginSettings
    {
        /// <summary>
        /// Gets or sets the allowed file extensions (including the period ".") that should be accessible from the browser.
        /// </summary>
        public ISet<string> BrowsableFileExtensions { get; set; } = new HashSet<string>(new[]
        {
            ".html", // markup
            ".css", // styles
            ".js", // scripts
            ".jpg", ".jpeg", ".gif", ".png", ".svg", // images
            ".eot", ".ttf", ".woff", // fonts
            ".xml", ".json", ".config", // configurations
            ".lic" // license
        });
    }
}
