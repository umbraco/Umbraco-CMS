using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class WebRoutingElement : ConfigurationElement, IWebRoutingSection
    {
        [ConfigurationProperty("trySkipIisCustomErrors", DefaultValue = "false")]
        public bool TrySkipIisCustomErrors => (bool) base["trySkipIisCustomErrors"];

        [ConfigurationProperty("internalRedirectPreservesTemplate", DefaultValue = "false")]
        public bool InternalRedirectPreservesTemplate => (bool) base["internalRedirectPreservesTemplate"];

        [ConfigurationProperty("disableAlternativeTemplates", DefaultValue = "false")]
        public bool DisableAlternativeTemplates => (bool) base["disableAlternativeTemplates"];

        [ConfigurationProperty("validateAlternativeTemplates", DefaultValue = "false")]
        public bool ValidateAlternativeTemplates => (bool) base["validateAlternativeTemplates"];

        [ConfigurationProperty("disableFindContentByIdPath", DefaultValue = "false")]
        public bool DisableFindContentByIdPath => (bool) base["disableFindContentByIdPath"];

        [ConfigurationProperty("disableRedirectUrlTracking", DefaultValue = "false")]
        public bool DisableRedirectUrlTracking => (bool) base["disableRedirectUrlTracking"];

        [ConfigurationProperty("urlProviderMode", DefaultValue = "Auto")]
        public string UrlProviderMode => (string) base["urlProviderMode"];

        [ConfigurationProperty("umbracoApplicationUrl", DefaultValue = null)]
        public string UmbracoApplicationUrl => (string)base["umbracoApplicationUrl"];
    }
}
