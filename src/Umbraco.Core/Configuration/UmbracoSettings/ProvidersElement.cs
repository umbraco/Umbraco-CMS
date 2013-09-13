using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class ProvidersElement : ConfigurationElement, IProviders
    {
        [ConfigurationProperty("users")]
        public UserProviderElement Users
        {
            get { return (UserProviderElement)base["users"]; }
        }

        IUserProvider IProviders.Users
        {
            get { return Users; }
        }
    }
}