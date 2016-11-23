using System;
using System.Collections.Generic;

namespace Umbraco.Core.Sync
{
    public class SingleServerRegistrar : IServerRegistrar
    {
        private readonly IRuntimeState _runtime;
        private readonly Lazy<IServerAddress[]> _registrations;

        public IEnumerable<IServerAddress> Registrations => _registrations.Value;

        public SingleServerRegistrar(IRuntimeState runtime)
        {
            _runtime = runtime;
            _registrations = new Lazy<IServerAddress[]>(() => new[] { new ServerAddressImpl(_runtime.ApplicationUrl.ToString()) });
        }

        public ServerRole GetCurrentServerRole()
        {
            return ServerRole.Single;
        }

        public string GetCurrentServerUmbracoApplicationUrl()
        {
            return _runtime.ApplicationUrl.ToString();
        }

        private class ServerAddressImpl : IServerAddress
        {
            public ServerAddressImpl(string serverAddress)
            {
                ServerAddress = serverAddress;
            }

            public string ServerAddress { get; }
        }
    }
}
