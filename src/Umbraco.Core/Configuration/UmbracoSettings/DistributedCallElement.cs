using System.Collections.Generic;
using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class DistributedCallElement : ConfigurationElement, IDistributedCall
    {
        [ConfigurationProperty("enable", DefaultValue = false)]
        internal bool Enabled
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
        internal ServerCollection Servers
        {
            get { return (ServerCollection)base["servers"]; }
        }

        bool IDistributedCall.Enabled
        {
            get { return Enabled; }
        }

        int IDistributedCall.UserId
        {
            get { return UserId; }
        }

        IEnumerable<IServer> IDistributedCall.Servers
        {
            get { return Servers; }
        }
    }
}