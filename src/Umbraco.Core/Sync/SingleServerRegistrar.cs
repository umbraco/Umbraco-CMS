using System;
using System.Collections.Generic;
using Umbraco.Web;

namespace Umbraco.Core.Sync
{
    /// <summary>
    /// Can be used when Umbraco is definitely not operating in a Load Balanced scenario to micro-optimize some startup performance
    /// </summary>
    /// <remarks>
    /// The micro optimization is specifically to avoid a DB query just after the app starts up to determine the <see cref="ServerRole"/>
    /// which by default is done with master election by a database query. The master election process doesn't occur until just after startup
    /// so this micro optimization doesn't really affect the primary startup phase.
    /// </remarks>
    public class SingleServerRegistrar : IServerRegistrar
    {
        private readonly IRequestAccessor _requestAccessor;
        private readonly Lazy<IServerAddress[]> _registrations;

        public IEnumerable<IServerAddress> Registrations => _registrations.Value;

        public SingleServerRegistrar(IRequestAccessor requestAccessor)
        {
            _requestAccessor = requestAccessor;
            _registrations = new Lazy<IServerAddress[]>(() => new IServerAddress[] { new ServerAddressImpl(_requestAccessor.GetApplicationUrl().ToString()) });
        }

        public ServerRole GetCurrentServerRole()
        {
            return ServerRole.Single;
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
