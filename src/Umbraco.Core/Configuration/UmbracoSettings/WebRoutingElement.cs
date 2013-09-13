using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class WebRoutingElement : ConfigurationElement, IWebRouting
    {
        [ConfigurationProperty("trySkipIisCustomErrors", DefaultValue = "false")]
        public bool TrySkipIisCustomErrors
        {
            get { return (bool) base["trySkipIisCustomErrors"]; }
        }

        [ConfigurationProperty("internalRedirectPreservesTemplate", DefaultValue = "false")]
        public bool InternalRedirectPreservesTemplate
        {
            get { return (bool) base["internalRedirectPreservesTemplate"]; }
        }

        [ConfigurationProperty("urlProviderMode", DefaultValue = "Auto")]
        public string UrlProviderMode
        {
            get { return (string)base["urlProviderMode"]; }
        }

    }
}