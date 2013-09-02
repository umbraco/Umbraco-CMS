using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class ProvidersElement : ConfigurationElement
    {
        [ConfigurationProperty("users")]
        public UserProviderElement Users
        {
            get { return (UserProviderElement)base["users"]; }
        }
    }
}