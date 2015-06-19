using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;

namespace Umbraco.Core.Sync
{
    /// <summary>
    /// Provides server registrations to the distributed cache by reading the legacy Xml configuration
    /// in umbracoSettings to get the list of (manually) configured server nodes.
    /// </summary>
    internal class ConfigServerRegistrar : IServerRegistrar
    {
        private readonly List<IServerAddress> _addresses;

        public ConfigServerRegistrar()
            : this(UmbracoConfig.For.UmbracoSettings().DistributedCall.Servers)
        { }

        internal ConfigServerRegistrar(IEnumerable<IServer> servers)
        {
            _addresses = servers == null
                ? new List<IServerAddress>()
                : servers
                    .Select(x => new ConfigServerAddress(x))
                    .Cast<IServerAddress>()
                    .ToList();
        }

        public IEnumerable<IServerAddress> Registrations
        {
            get { return _addresses; }
        }
    }
}
