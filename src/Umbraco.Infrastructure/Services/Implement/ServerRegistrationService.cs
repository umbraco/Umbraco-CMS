using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services.Implement
{
    /// <summary>
    /// Manages server registrations in the database.
    /// </summary>
    public sealed class ServerRegistrationService : RepositoryService, IServerRegistrationService
    {
        private readonly IServerRegistrationRepository _serverRegistrationRepository;
        private readonly IHostingEnvironment _hostingEnvironment;

        private ServerRole _currentServerRole = ServerRole.Unknown;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerRegistrationService"/> class.
        /// </summary>
        public ServerRegistrationService(
            IScopeProvider scopeProvider,
            ILoggerFactory loggerFactory,
            IEventMessagesFactory eventMessagesFactory,
            IServerRegistrationRepository serverRegistrationRepository,
            IHostingEnvironment hostingEnvironment)
            : base(scopeProvider, loggerFactory, eventMessagesFactory)
        {
            _serverRegistrationRepository = serverRegistrationRepository;
            _hostingEnvironment = hostingEnvironment;
        }

        /// <summary>
        /// Touches a server to mark it as active; deactivate stale servers.
        /// </summary>
        /// <param name="serverAddress">The server URL.</param>
        /// <param name="staleTimeout">The time after which a server is considered stale.</param>
        public void TouchServer(string serverAddress, TimeSpan staleTimeout)
        {
            var serverIdentity = GetCurrentServerIdentity();
            using (var scope = ScopeProvider.CreateScope())
            {
                scope.WriteLock(Cms.Core.Constants.Locks.Servers);

                ((ServerRegistrationRepository) _serverRegistrationRepository).ClearCache(); // ensure we have up-to-date cache

                var regs = _serverRegistrationRepository.GetMany().ToArray();
                var hasMaster = regs.Any(x => ((ServerRegistration) x).IsMaster);
                var server = regs.FirstOrDefault(x => x.ServerIdentity.InvariantEquals(serverIdentity));

                if (server == null)
                {
                    server = new ServerRegistration(serverAddress, serverIdentity, DateTime.Now);
                }
                else
                {
                    server.ServerAddress = serverAddress; // should not really change but it might!
                    server.UpdateDate = DateTime.Now;
                }

                server.IsActive = true;
                if (hasMaster == false)
                    server.IsMaster = true;

                _serverRegistrationRepository.Save(server);
                _serverRegistrationRepository.DeactiveStaleServers(staleTimeout); // triggers a cache reload

                // reload - cheap, cached

                // default role is single server, but if registrations contain more
                // than one active server, then role is master or replica
                regs = _serverRegistrationRepository.GetMany().ToArray();

                // default role is single server, but if registrations contain more
                // than one active server, then role is master or replica
                _currentServerRole = regs.Count(x => x.IsActive) > 1
                    ? (server.IsMaster ? ServerRole.Master : ServerRole.Replica)
                    : ServerRole.Single;

                scope.Complete();
            }
        }

        /// <summary>
        /// Deactivates a server.
        /// </summary>
        /// <param name="serverIdentity">The server unique identity.</param>
        public void DeactiveServer(string serverIdentity)
        {
            // because the repository caches "all" and has queries disabled...

            using (var scope = ScopeProvider.CreateScope())
            {
                scope.WriteLock(Cms.Core.Constants.Locks.Servers);

                ((ServerRegistrationRepository) _serverRegistrationRepository).ClearCache(); // ensure we have up-to-date cache // ensure we have up-to-date cache

                var server = _serverRegistrationRepository.GetMany().FirstOrDefault(x => x.ServerIdentity.InvariantEquals(serverIdentity));
                if (server == null) return;
                server.IsActive = server.IsMaster = false;
                _serverRegistrationRepository.Save(server); // will trigger a cache reload // will trigger a cache reload

                scope.Complete();
            }
        }

        /// <summary>
        /// Deactivates stale servers.
        /// </summary>
        /// <param name="staleTimeout">The time after which a server is considered stale.</param>
        public void DeactiveStaleServers(TimeSpan staleTimeout)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                scope.WriteLock(Cms.Core.Constants.Locks.Servers);
                _serverRegistrationRepository.DeactiveStaleServers(staleTimeout);
                scope.Complete();
            }
        }

        /// <summary>
        /// Return all active servers.
        /// </summary>
        /// <param name="refresh">A value indicating whether to force-refresh the cache.</param>
        /// <returns>All active servers.</returns>
        /// <remarks>By default this method will rely on the repository's cache, which is updated each
        /// time the current server is touched, and the period depends on the configuration. Use the
        /// <paramref name="refresh"/> parameter to force a cache refresh and reload active servers
        /// from the database.</remarks>
        public IEnumerable<IServerRegistration> GetActiveServers(bool refresh = false)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Cms.Core.Constants.Locks.Servers);
                if (refresh) ((ServerRegistrationRepository) _serverRegistrationRepository).ClearCache();
                return _serverRegistrationRepository.GetMany().Where(x => x.IsActive).ToArray(); // fast, cached // fast, cached
            }
        }

        /// <summary>
        /// Gets the role of the current server.
        /// </summary>
        /// <returns>The role of the current server.</returns>
        public ServerRole GetCurrentServerRole() => _currentServerRole;

        /// <summary>
        /// Gets the local server identity.
        /// </summary>
        private string GetCurrentServerIdentity() => Environment.MachineName // eg DOMAIN\SERVER
                                               + "/" + _hostingEnvironment.ApplicationId; // eg /LM/S3SVC/11/ROOT;
    }
}
