using Umbraco.Core.Configuration.UmbracoSettings;

namespace Umbraco.Configuration.Implementations
{
    internal class WebRoutingSettings : ConfigurationManagerConfigBase, IWebRoutingSettings
    {
        public bool TrySkipIisCustomErrors => UmbracoSettingsSection.WebRouting.TrySkipIisCustomErrors;
        public bool InternalRedirectPreservesTemplate => UmbracoSettingsSection.WebRouting.InternalRedirectPreservesTemplate;
        public bool DisableAlternativeTemplates => UmbracoSettingsSection.WebRouting.DisableAlternativeTemplates;
        public bool ValidateAlternativeTemplates => UmbracoSettingsSection.WebRouting.ValidateAlternativeTemplates;
        public bool DisableFindContentByIdPath => UmbracoSettingsSection.WebRouting.DisableFindContentByIdPath;
        public bool DisableRedirectUrlTracking => UmbracoSettingsSection.WebRouting.DisableRedirectUrlTracking;
        public string UrlProviderMode => UmbracoSettingsSection.WebRouting.UrlProviderMode;
        public string UmbracoApplicationUrl => UmbracoSettingsSection.WebRouting.UmbracoApplicationUrl;
    }
}
