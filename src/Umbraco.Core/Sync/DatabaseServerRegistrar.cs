using System.Collections.Generic;
using Umbraco.Core.Services;

namespace Umbraco.Core.Sync
{
   
    /// <summary>
    /// A registrar that stores registered server nodes in a database
    /// </summary>
    internal class DatabaseServerRegistrar : IServerRegistrar
    {
        private readonly ServerRegistrationService _registrationService;

        public DatabaseServerRegistrar(ServerRegistrationService registrationService)
        {
            _registrationService = registrationService;
        }

        public IEnumerable<IServerAddress> Registrations
        {
            get { return _registrationService.GetActiveServers(); }
        }
    }
}