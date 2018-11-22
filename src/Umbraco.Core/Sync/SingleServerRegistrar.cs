using System.Collections.Generic;

namespace Umbraco.Core.Sync
{
    public class SingleServerRegistrar : IServerRegistrar2
    {
        private readonly string _umbracoApplicationUrl;

        public IEnumerable<IServerAddress> Registrations { get; private set; }

        public SingleServerRegistrar()
        {
            _umbracoApplicationUrl = ApplicationContext.Current.UmbracoApplicationUrl;
            Registrations = new[] { new ServerAddressImpl(_umbracoApplicationUrl) };
        }

        public ServerRole GetCurrentServerRole()
        {
            return ServerRole.Single;
        }

        public string GetCurrentServerUmbracoApplicationUrl()
        {
            return _umbracoApplicationUrl;
        }

        private class ServerAddressImpl : IServerAddress
        {
            public ServerAddressImpl(string serverAddress)
            {
                ServerAddress = serverAddress;
            }

            public string ServerAddress { get; private set; }
        }
    }
}
