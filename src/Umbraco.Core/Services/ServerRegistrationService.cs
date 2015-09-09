using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
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
                ((ServerRegistrationRepository) xr.Repository).ReloadCache(); // ensure we have up-to-date cache

                var regs = xr.Repository.GetAll().ToArray();
                var hasMaster = regs.Any(x => ((ServerRegistration)x).IsMaster);
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

                xr.Repository.AddOrUpdate(server);
                xr.UnitOfWork.Commit(); // triggers a cache reload
                xr.Repository.DeactiveStaleServers(staleTimeout); // triggers a cache reload

                // reload - cheap, cached
                regs = xr.Repository.GetAll().ToArray();

                // default role is single server, but if registrations contain more
                // than one active server, then role is master or slave
                _currentServerRole = regs.Count(x => x.IsActive) > 1
                    ? (server.IsMaster ? ServerRole.Master : ServerRole.Slave)
                    : ServerRole.Single;
            });
        }

        /// <summary>
        /// Deactivates a server.
        /// </summary>
        /// <param name="serverIdentity">The server unique identity.</param>
        public void DeactiveServer(string serverIdentity)
        {
            //_lrepo.WithWriteLocked(xr =>
            //{
            //    var query = Query<IServerRegistration>.Builder.Where(x => x.ServerIdentity.ToUpper() == serverIdentity.ToUpper());
            //    var server = xr.Repository.GetByQuery(query).FirstOrDefault();
            //    if (server == null) return;

            //    server.IsActive = false;
            //    server.IsMaster = false;
            //    xr.Repository.AddOrUpdate(server);
            //});

            // because the repository caches "all" and has queries disabled...

            _lrepo.WithWriteLocked(xr =>
            {
                ((ServerRegistrationRepository)xr.Repository).ReloadCache(); // ensure we have up-to-date cache

                var server = xr.Repository.GetAll().FirstOrDefault(x => x.ServerIdentity.InvariantEquals(serverIdentity));
                if (server == null) return;
                server.IsActive = server.IsMaster = false;
                xr.Repository.AddOrUpdate(server); // will trigger a cache reload
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
            //return _lrepo.WithReadLocked(xr =>
            //{
            //    var query = Query<IServerRegistration>.Builder.Where(x => x.IsActive);
            //    return xr.Repository.GetByQuery(query).ToArray();
            //});

            // because the repository caches "all" we should use the following code
            // in order to ensure we use the cache and not hit the database each time

            //return _lrepo.WithReadLocked(xr => xr.Repository.GetAll().Where(x => x.IsActive).ToArray());

            // however, WithReadLocked (as any other LockingRepository methods) will attempt
            // to properly lock the repository using a database-level lock, which wants
            // the transaction isolation level to be RepeatableRead, which it is not by default,
            // and then, see U4-7046.
            //
            // in addition, LockingRepository methods need to hit the database in order to
            // ensure proper locking, and so if we know that the repository might not need the
            // database, we cannot use these methods - and then what?
            //
            // this raises a good number of questions, including whether caching anything in
            // repositories works at all in a LB environment - TODO: figure it out

            var uow = UowProvider.GetUnitOfWork();
            var repo = RepositoryFactory.CreateServerRegistrationRepository(uow);
            return repo.GetAll().Where(x => x.IsActive).ToArray(); // fast, cached
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