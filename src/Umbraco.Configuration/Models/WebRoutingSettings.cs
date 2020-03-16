using Microsoft.Extensions.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Configuration.Models
{
    internal class WebRoutingSettings :  IWebRoutingSettings
    {
        private readonly IConfiguration _configuration;
        public WebRoutingSettings(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public bool TrySkipIisCustomErrors => _configuration.GetValue<bool?>("Umbraco:CMS:WebRouting:TrySkipIisCustomErrors") ?? false;
        public bool InternalRedirectPreservesTemplate => _configuration.GetValue<bool?>("Umbraco:CMS:WebRouting:InternalRedirectPreservesTemplate") ?? false;
        public bool DisableAlternativeTemplates => _configuration.GetValue<bool?>("Umbraco:CMS:WebRouting:DisableAlternativeTemplates") ?? false;
        public bool ValidateAlternativeTemplates => _configuration.GetValue<bool?>("Umbraco:CMS:WebRouting:ValidateAlternativeTemplates") ?? false;
        public bool DisableFindContentByIdPath => _configuration.GetValue<bool?>("Umbraco:CMS:WebRouting:DisableFindContentByIdPath") ?? false;
        public bool DisableRedirectUrlTracking => _configuration.GetValue<bool?>("Umbraco:CMS:WebRouting:DisableRedirectUrlTracking") ?? false;
        public string UrlProviderMode =>  _configuration.GetValue<string?>("Umbraco:CMS:WebRouting:UrlProviderMode") ?? UrlMode.Auto.ToString();
        public string UmbracoApplicationUrl => _configuration.GetValue<string?>("Umbraco:CMS:WebRouting:UmbracoApplicationUrl") ?? null;
    }
}
