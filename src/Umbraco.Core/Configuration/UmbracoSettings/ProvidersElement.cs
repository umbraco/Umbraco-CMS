using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class ProvidersElement : ConfigurationElement, IProvidersSection
    {
        [ConfigurationProperty("users")]
        public UserProviderElement Users
        {
            get { return (UserProviderElement)base["users"]; }
        }

        public string DefaultBackOfficeUserProvider
        {
            get { return Users.DefaultBackOfficeProvider; }
        }
    }
}