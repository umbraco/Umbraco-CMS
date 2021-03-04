// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.Configuration.Models
{
    /// <summary>
    /// Typed configuration options for web routing settings.
    /// </summary>
    public class WebRoutingSettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether to check if any routed endpoints match a front-end request before
        /// the Umbraco dynamic router tries to map the request to an Umbraco content item.
        /// </summary>
        /// <remarks>
        /// This should not be necessary if the Umbraco catch-all/dynamic route is registered last like it's supposed to be. In that case
        /// ASP.NET Core will automatically handle this in all cases. This is more of a backward compatible option since this is what v7/v8 used
        /// to do.
        /// </remarks>
        public bool TryMatchingEndpointsForAllPages { get; set; } = false;

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
