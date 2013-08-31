using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class DistributedCallElement : ConfigurationElement
    {
        [ConfigurationProperty("enable")]
        public bool Enabled
        {
            get { return (bool)base["enable"]; }
        }

        [ConfigurationProperty("user")]
        internal InnerTextConfigurationElement<int> UserId
        {
            get { return (InnerTextConfigurationElement<int>)this["user"]; }
        }

        [ConfigurationCollection(typeof(ServerCollection), AddItemName = "server")]
        [ConfigurationProperty("servers", IsDefaultCollection = true)]
        public ServerCollection Servers
        {
            get { return (ServerCollection)base["servers"]; }
        }
    }
}