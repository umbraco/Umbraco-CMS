using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Sync;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Manages server registrations in the database.
    /// </summary>
    public sealed class ServerRegistrationService : RepositoryService, IServerRegistrationService
    {
        private readonly static string CurrentServerIdentityValue = NetworkHelper.MachineName // eg DOMAIN\SERVER
                                                            + "/" + HttpRuntime.AppDomainAppId; // eg /LM/S3SVC/11/ROOT

        private static readonly int[] LockingRepositoryIds = { Constants.System.ServersLock };
        private ServerRole _currentServerRole = ServerRole.Unknown;
        private readonly LockingRepository<IServerRegistrationRepository> _lrepo;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerRegistrationService"/> class.
        /// </summary>
        /// <param name="uowProvider">A UnitOfWork provider.</param>
        /// <param name="repositoryFactory">A repository factory.</param>
        /// <param name="logger">A logger.</param>
        /// <param name="eventMessagesFactory"></param>
        public ServerRegistrationService(IDatabaseUnitOfWorkProvider uowProvider, RepositoryFactory repositoryFactory, ILogger logger, IEventMessagesFactory eventMessagesFactory)
            : base(uowProvider, repositoryFactory, logger, eventMessagesFactory)
        {
            _lrepo = new LockingRepository<IServerRegistrationRepository>(UowProvider,
                x => RepositoryFactory.CreateServerRegistrationRepository(x),
                LockingRepositoryIds, LockingRepositoryIds);

        }

        /// <summary>
        /// Touches a server to mark it as active; deactivate stale servers.
        /// </summary>
        /// <param name="serverAddress">The server url.</param>
        /// <param name="serverIdentity">The server unique identity.</param>
        /// <param name="staleTimeout">The time after which a server is considered stale.</param>
        public void TouchServer(string serverAddress, string serverIdentity, TimeSpan staleTimeout)
        {
            _lrepo.WithWriteLocked(xr =>
            {
                var regs = xr.Repository.GetAll().ToArray(); // faster to query only once
                var hasMaster = regs.Any(x => ((ServerRegistration)x).IsMaster);
                var iserver = regs.FirstOrDefault(x => x.ServerIdentity.InvariantEquals(serverIdentity));
                var server = iserver as ServerRegistration; // because IServerRegistration is missing IsMaster
                var hasServer = server != null;

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

                xr.Repository.AddOrUpdate(server);
                xr.UnitOfWork.Commit();
                xr.Repository.DeactiveStaleServers(staleTimeout);

                // default role is single server
                _currentServerRole = ServerRole.Single;

                // if registrations contain more than 0/1 server, role is master or slave
                // compare to 0 or 1 depending on whether regs already contains the server
                if (regs.Length > (hasServer ? 1 : 0))
                    _currentServerRole = server.IsMaster
                        ? ServerRole.Master
                        : ServerRole.Slave;
            });
        }

        /// <summary>
        /// Deactivates a server.
        /// </summary>
        /// <param name="serverIdentity">The server unique identity.</param>
        public void DeactiveServer(string serverIdentity)
        {
            _lrepo.WithWriteLocked(xr =>
            {
                var query = Query<IServerRegistration>.Builder.Where(x => x.ServerIdentity.ToUpper() == serverIdentity.ToUpper());
                var iserver = xr.Repository.GetByQuery(query).FirstOrDefault();
                var server = iserver as ServerRegistration; // because IServerRegistration is missing IsMaster
                if (server == null) return;

                server.IsActive = false;
                server.IsMaster = false;
                xr.Repository.AddOrUpdate(server);
            });
        }

        /// <summary>
        /// Deactivates stale servers.
        /// </summary>
        /// <param name="staleTimeout">The time after which a server is considered stale.</param>
        public void DeactiveStaleServers(TimeSpan staleTimeout)
        {
            _lrepo.WithWriteLocked(xr => xr.Repository.DeactiveStaleServers(staleTimeout));
        }

        /// <summary>
        /// Return all active servers.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IServerRegistration> GetActiveServers()
        {
            return _lrepo.WithReadLocked(xr =>
            {
                var query = Query<IServerRegistration>.Builder.Where(x => x.IsActive);
                return xr.Repository.GetByQuery(query).ToArray();
            });
        }

        /// <summary>
        /// Gets the local server identity.
        /// </summary>
        public string CurrentServerIdentity { get { return CurrentServerIdentityValue; } }

        /// <summary>
        /// Gets the role of the current server.
        /// </summary>
        /// <returns>The role of the current server.</returns>
        public ServerRole GetCurrentServerRole()
        {
            return _currentServerRole;
        }
    }
}