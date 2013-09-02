using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class RequestHandlerElement : ConfigurationElement
    {
        [ConfigurationProperty("useDomainPrefixes")]
        internal InnerTextConfigurationElement<bool> UseDomainPrefixes
        {
            get
            {
                return new OptionalInnerTextConfigurationElement<bool>(
                    (InnerTextConfigurationElement<bool>)this["useDomainPrefixes"],
                    //set the default
                    false);  
            }
        }

        [ConfigurationProperty("addTrailingSlash")]
        internal InnerTextConfigurationElement<bool> AddTrailingSlash
        {
            get
            {
                return new OptionalInnerTextConfigurationElement<bool>(
                    (InnerTextConfigurationElement<bool>)this["addTrailingSlash"],
                    //set the default
                    true);  
            }
        }

        [ConfigurationProperty("urlReplacing")]
        internal UrlReplacingElement UrlReplacing
        {
            get { return (UrlReplacingElement)this["urlReplacing"]; }
        }
    }
}