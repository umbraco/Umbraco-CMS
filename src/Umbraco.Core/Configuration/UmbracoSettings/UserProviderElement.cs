using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class UserProviderElement : ConfigurationElement
    {
        [ConfigurationProperty("DefaultBackofficeProvider")]
        internal InnerTextConfigurationElement<string> DefaultBackOfficeProvider
        {
            get { return (InnerTextConfigurationElement<string>)this["DefaultBackofficeProvider"]; }
        }
    }
}