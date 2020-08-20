using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Configuration.Models
{
    public class WebRoutingSettings
    {
        public bool TrySkipIisCustomErrors { get; set; } = false;

        public bool InternalRedirectPreservesTemplate { get; set; } = false;

        public bool DisableAlternativeTemplates { get; set; } = false;

        public bool ValidateAlternativeTemplates { get; set; } = false;

        public bool DisableFindContentByIdPath { get; set; } = false;

        public bool DisableRedirectUrlTracking { get; set; } = false;

        public string UrlProviderMode { get; set; } = UrlMode.Auto.ToString();

        public string UmbracoApplicationUrl { get; set; }
    }
}
