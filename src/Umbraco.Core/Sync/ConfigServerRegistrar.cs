using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Models;

namespace Umbraco.Core.Sync
{
    /// <summary>
    /// A registrar that uses the legacy xml configuration in umbracoSettings to get a list of defined server nodes
    /// </summary>
    internal class ConfigServerRegistrar : IServerRegistrar
    {
        private readonly IEnumerable<IServer> _servers;

        public ConfigServerRegistrar()
            : this(UmbracoConfig.For.UmbracoSettings().DistributedCall.Servers)
        {
            
        }

        internal ConfigServerRegistrar(IEnumerable<IServer> servers)
        {
            _servers = servers;
        }

        private List<IServerAddress> _addresses;

        public IEnumerable<IServerAddress> Registrations
        {
            get
            {
                if (_addresses == null)
                {
                    _addresses = new List<IServerAddress>();
                    
                    if (_servers != null)
                    {
                        foreach (var n in _servers)
                        {
                            _addresses.Add(new ConfigServerAddress(n));
                        } 
                    }
                }

                return _addresses;
            }
        }
    }
}
