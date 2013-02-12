using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Umbraco.Core.Configuration;

namespace Umbraco.Core.Sync
{
    /// <summary>
    /// A registrar that uses the legacy xml configuration in umbracoSettings to get a list of defined server nodes
    /// </summary>
    internal class ConfigServerRegistrar : IServerRegistrar
    {
        private readonly XmlNode _xmlServers;

        public ConfigServerRegistrar()
            : this(UmbracoSettings.DistributionServers)
        {
            
        }

        internal ConfigServerRegistrar(XmlNode xmlServers)
        {
            _xmlServers = xmlServers;
        }

        private List<IServerAddress> _addresses;

        public IEnumerable<IServerAddress> Registrations
        {
            get
            {
                if (_addresses == null)
                {
                    _addresses = new List<IServerAddress>();
                    var nodes = _xmlServers.SelectNodes("./server");
                    foreach (XmlNode n in nodes)
                     {
                         _addresses.Add(new ConfigServerAddress(n));
                     }
                }
                return _addresses;
            }
        }
    }
}
