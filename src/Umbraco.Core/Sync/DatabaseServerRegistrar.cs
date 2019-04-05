using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Services;

namespace Umbraco.Core.Sync
{

    public class MasterServerRegistrar : IServerRegistrar
    {
        public IEnumerable<IServerAddress> Registrations
        {
            get { return Enumerable.Empty<IServerAddress>(); }
        }
        public ServerRole GetCurrentServerRole()
        {
            return ServerRole.Master;
        }
        public string GetCurrentServerUmbracoApplicationUrl()
        {
            // NOTE: If you want to explicitly define the URL that your application is running on,
            // this will be used for the server to communicate with itself, you can return the
            // custom path here and it needs to be in this format:
            // http://www.mysite.com/umbraco

            return null;
        }
    }

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
        /// Gets the current umbraco application url.
        /// </summary>
        public string GetCurrentServerUmbracoApplicationUrl()
        {
            // this registrar does not provide the umbraco application url
            return null;
        }

    }
}
