using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class DistributedCallElement : ConfigurationElement
    {
        [ConfigurationProperty("enable", DefaultValue = false)]
        public bool Enabled
        {
            get { return (bool)base["enable"]; }
        }

        [ConfigurationProperty("user")]
        internal InnerTextConfigurationElement<int> UserId
        {
            get
            {
                return new OptionalInnerTextConfigurationElement<int>(
                       (InnerTextConfigurationElement<int>)this["user"],
                    //set the default
                       0);
            }
        }

        [ConfigurationCollection(typeof(ServerCollection), AddItemName = "server")]
        [ConfigurationProperty("servers", IsDefaultCollection = true)]
        public ServerCollection Servers
        {
            get { return (ServerCollection)base["servers"]; }
        }
    }
}