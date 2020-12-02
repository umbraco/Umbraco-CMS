using System;
using System.Collections.Generic;
using Umbraco.Core.Services;

namespace Umbraco.Core.Sync
{
    /// <summary>
    /// A registrar that stores registered server nodes in the database.
    /// </summary>
    /// <remarks>
    /// This is the default registrar which determines a server's role by using a master election process.
    /// The master election process doesn't occur until just after startup so this election process doesn't really affect the primary startup phase.
    /// </remarks>
    public sealed class DatabaseServerRegistrar : IServerRegistrar
    {
        private readonly Lazy<IServerRegistrationService> _registrationService;

        /// <summary>
        /// Gets or sets the registrar options.
        /// </summary>
        public DatabaseServerRegistrarOptions Options { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseServerRegistrar"/> class.
        /// </summary>
        /// <param name="registrationService">The registration service.</param>
        /// <param name="options">Some options.</param>
        public DatabaseServerRegistrar(Lazy<IServerRegistrationService> registrationService, DatabaseServerRegistrarOptions options)
        {
            Options = options ?? throw new ArgumentNullException(nameof(options));
            _registrationService = registrationService ?? throw new ArgumentNullException(nameof(registrationService));
        }

        /// <summary>
        /// Gets the registered servers.
        /// </summary>
        public IEnumerable<IServerAddress> Registrations => _registrationService.Value.GetActiveServers();

        /// <summary>
        /// Gets the role of the current server in the application environment.
        /// </summary>
        public ServerRole GetCurrentServerRole()
        {
            var service = _registrationService.Value;
            return service.GetCurrentServerRole();
        }

        /// <summary>
        /// Gets the current umbraco application URL.
        /// </summary>
        public string GetCurrentServerUmbracoApplicationUrl()
        {
            // this registrar does not provide the umbraco application URL
            return null;
        }

    }
}
