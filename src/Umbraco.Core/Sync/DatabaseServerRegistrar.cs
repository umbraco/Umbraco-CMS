using System;
using System.Collections.Generic;
using Umbraco.Core.Services;

namespace Umbraco.Core.Sync
{
    /// <summary>
    /// A registrar that stores registered server nodes in the database.
    /// </summary>
    public sealed class DatabaseServerRegistrar : IServerRegistrar2
    {
        private readonly Lazy<IServerRegistrationService> _registrationService;

        /// <summary>
        /// Gets or sets the registrar options.
        /// </summary>
        public DatabaseServerRegistrarOptions Options { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseServerRegistrar"/> class.
        /// </summary>
        /// <param name="registrationService">The registration service.</param>
        /// <param name="options">Some options.</param>
        public DatabaseServerRegistrar(Lazy<IServerRegistrationService> registrationService, DatabaseServerRegistrarOptions options)
        {
            if (registrationService == null) throw new ArgumentNullException("registrationService");
            if (options == null) throw new ArgumentNullException("options");

            Options = options;
            _registrationService = registrationService;
        }

        /// <summary>
        /// Gets the registered servers.
        /// </summary>
        public IEnumerable<IServerAddress> Registrations
        {
            get { return _registrationService.Value.GetActiveServers(); }
        }

        /// <summary>
        /// Gets the role of the current server in the application environment.
        /// </summary>
        public ServerRole GetCurrentServerRole()
        {
            var service = _registrationService.Value;
            return service.GetCurrentServerRole();
        }

        /// <summary>
        /// Gets the current umbraco application url.
        /// </summary>
        public string GetCurrentServerUmbracoApplicationUrl()
        {
            // this registrar does not provide the umbraco application url
            return null;
        }

    }
}