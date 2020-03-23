using Microsoft.Extensions.Configuration;
using Umbraco.Core;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Configuration.Models
{
    internal class WebRoutingSettings : IWebRoutingSettings
    {
        private const string Prefix = Constants.Configuration.ConfigPrefix + "WebRouting:";
        private readonly IConfiguration _configuration;

        public WebRoutingSettings(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public bool TrySkipIisCustomErrors =>
            _configuration.GetValue(Prefix + "TrySkipIisCustomErrors", false);

        public bool InternalRedirectPreservesTemplate =>
            _configuration.GetValue(Prefix + "InternalRedirectPreservesTemplate", false);

        public bool DisableAlternativeTemplates =>
            _configuration.GetValue(Prefix + "DisableAlternativeTemplates", false);

        public bool ValidateAlternativeTemplates =>
            _configuration.GetValue(Prefix + "ValidateAlternativeTemplates", false);

        public bool DisableFindContentByIdPath =>
            _configuration.GetValue(Prefix + "DisableFindContentByIdPath", false);

        public bool DisableRedirectUrlTracking =>
            _configuration.GetValue(Prefix + "DisableRedirectUrlTracking", false);

        public string UrlProviderMode =>
            _configuration.GetValue(Prefix + "UrlProviderMode", UrlMode.Auto.ToString());

        public string UmbracoApplicationUrl =>
            _configuration.GetValue<string>(Prefix + "UmbracoApplicationUrl");
    }
}
