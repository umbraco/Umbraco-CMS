using System;
using System.Collections.Generic;
using Umbraco.Core.Services;

namespace Umbraco.Core.Sync
{
   
    /// <summary>
    /// A registrar that stores registered server nodes in a database
    /// </summary>
    internal class DatabaseServerRegistrar : IServerRegistrar
    {
        private readonly Lazy<ServerRegistrationService> _registrationService;

        public DatabaseServerRegistrar(Lazy<ServerRegistrationService> registrationService)
        {
            _registrationService = registrationService;
        }

        public IEnumerable<IServerAddress> Registrations
        {
            get { return _registrationService.Value.GetActiveServers(); }
        }
    }
}