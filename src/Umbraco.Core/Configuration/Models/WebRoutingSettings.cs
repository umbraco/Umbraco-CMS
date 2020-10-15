using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Core.Configuration.Models
{
    public class WebRoutingSettings
    {
        public bool TrySkipIisCustomErrors { get; set; } = false;

        public bool InternalRedirectPreservesTemplate { get; set; } = false;

        public bool DisableAlternativeTemplates { get; set; } = false;

        public bool ValidateAlternativeTemplates { get; set; } = false;

        public bool DisableFindContentByIdPath { get; set; } = false;

        public bool DisableRedirectUrlTracking { get; set; } = false;

        public UrlMode UrlProviderMode { get; set; } = UrlMode.Auto;

        public string UmbracoApplicationUrl { get; set; }
    }
}
