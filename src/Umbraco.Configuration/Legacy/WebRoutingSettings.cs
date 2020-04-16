using Umbraco.Core.Configuration.UmbracoSettings;

namespace Umbraco.Configuration.Implementations
{
    internal class WebRoutingSettings : ConfigurationManagerConfigBase, IWebRoutingSettings
    {
        public bool TrySkipIisCustomErrors => UmbracoSettingsSection?.WebRouting?.TrySkipIisCustomErrors ?? false;
        public bool InternalRedirectPreservesTemplate => UmbracoSettingsSection?.WebRouting?.InternalRedirectPreservesTemplate ?? false;
        public bool DisableAlternativeTemplates => UmbracoSettingsSection?.WebRouting?.DisableAlternativeTemplates ?? false;
        public bool ValidateAlternativeTemplates => UmbracoSettingsSection?.WebRouting?.ValidateAlternativeTemplates ?? false;
        public bool DisableFindContentByIdPath => UmbracoSettingsSection?.WebRouting?.DisableFindContentByIdPath ?? false;
        public bool DisableRedirectUrlTracking => UmbracoSettingsSection?.WebRouting?.DisableRedirectUrlTracking ?? false;
        public string UrlProviderMode => UmbracoSettingsSection?.WebRouting?.UrlProviderMode ?? "Auto";
        public string UmbracoApplicationUrl => UmbracoSettingsSection?.WebRouting?.UmbracoApplicationUrl;
    }
}
