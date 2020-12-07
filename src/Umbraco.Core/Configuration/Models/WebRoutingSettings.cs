// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Core.Configuration.Models
{
    /// <summary>
    /// Typed configuration options for web routing settings.
    /// </summary>
    public class WebRoutingSettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether IIS custom errors should be skipped.
        /// </summary>
        public bool TrySkipIisCustomErrors { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether an internal redirect should preserve the template.
        /// </summary>
        public bool InternalRedirectPreservesTemplate { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether the use of alternative templates are disabled.
        /// </summary>
        public bool DisableAlternativeTemplates { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether the use of alternative templates should be validated.
        /// </summary>
        public bool ValidateAlternativeTemplates { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether find content ID by path is disabled.
        /// </summary>
        public bool DisableFindContentByIdPath { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether redirect URL tracking is disabled.
        /// </summary>
        public bool DisableRedirectUrlTracking { get; set; } = false;

        /// <summary>
        /// Gets or sets a value for the URL provider mode (<see cref="UrlMode"/>).
        /// </summary>
        public UrlMode UrlProviderMode { get; set; } = UrlMode.Auto;

        /// <summary>
        /// Gets or sets a value for the Umbraco application URL.
        /// </summary>
        public string UmbracoApplicationUrl { get; set; }
    }
}
