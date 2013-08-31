using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class RequestHandlerElement : ConfigurationElement
    {
        [ConfigurationProperty("useDomainPrefixes")]
        internal InnerTextConfigurationElement<bool> UseDomainPrefixes
        {
            get { return (InnerTextConfigurationElement<bool>)this["useDomainPrefixes"]; }
        }

        [ConfigurationProperty("addTrailingSlash")]
        internal InnerTextConfigurationElement<bool> AddTrailingSlash
        {
            get { return (InnerTextConfigurationElement<bool>)this["addTrailingSlash"]; }
        }

        [ConfigurationProperty("urlReplacing")]
        internal UrlReplacingElement UrlReplacing
        {
            get { return (UrlReplacingElement)this["urlReplacing"]; }
        }
    }
}