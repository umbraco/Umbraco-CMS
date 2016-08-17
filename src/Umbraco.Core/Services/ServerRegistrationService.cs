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

        private ServerRole _currentServerRole = ServerRole.Unknown;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerRegistrationService"/> class.
        /// </summary>
        /// <param name="uowProvider">A UnitOfWork provider.</param>
        /// <param name="logger">A logger.</param>
        /// <param name="eventMessagesFactory"></param>
        public ServerRegistrationService(IDatabaseUnitOfWorkProvider uowProvider, ILogger logger, IEventMessagesFactory eventMessagesFactory)
            : base(uowProvider, logger, eventMessagesFactory)
        { }

        /// <summary>
        /// Touches a server to mark it as active; deactivate stale servers.
        /// </summary>
        /// <param name="serverAddress">The server url.</param>
        /// <param name="serverIdentity">The server unique identity.</param>
        /// <param name="staleTimeout">The time after which a server is considered stale.</param>
        public void TouchServer(string serverAddress, string serverIdentity, TimeSpan staleTimeout)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.WriteLock(Constants.Locks.Servers);
                var repo = uow.CreateRepository<IServerRegistrationRepository>();

                ((ServerRegistrationRepository) repo).ReloadCache(); // ensure we have up-to-date cache

                var regs = repo.GetAll().ToArray();
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

                repo.AddOrUpdate(server);
                uow.Flush(); // triggers a cache reload
                repo.DeactiveStaleServers(staleTimeout); // triggers a cache reload

                // reload - cheap, cached
                regs = repo.GetAll().ToArray();

                // default role is single server, but if registrations contain more
                // than one active server, then role is master or slave
                _currentServerRole = regs.Count(x => x.IsActive) > 1
                        ? (server.IsMaster ? ServerRole.Master : ServerRole.Slave)
                        : ServerRole.Single;

                uow.Complete();
            }
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

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.WriteLock(Constants.Locks.Servers);
                var repo = uow.CreateRepository<IServerRegistrationRepository>();

                ((ServerRegistrationRepository) repo).ReloadCache(); // ensure we have up-to-date cache

                var server = repo.GetAll().FirstOrDefault(x => x.ServerIdentity.InvariantEquals(serverIdentity));
                if (server == null) return;
                server.IsActive = server.IsMaster = false;
                repo.AddOrUpdate(server); // will trigger a cache reload

                uow.Complete();
            }
        }

        /// <summary>
        /// Deactivates stale servers.
        /// </summary>
        /// <param name="staleTimeout">The time after which a server is considered stale.</param>
        public void DeactiveStaleServers(TimeSpan staleTimeout)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.WriteLock(Constants.Locks.Servers);
                var repo = uow.CreateRepository<IServerRegistrationRepository>();
                repo.DeactiveStaleServers(staleTimeout);
                uow.Complete();
            }
        }

        /// <summary>
        /// Return all active servers.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IServerRegistration> GetActiveServers()
        {
            // fixme - this needs to be refactored entirely now that we have repeatable read everywhere

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

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.Servers);
                var repo = uow.CreateRepository<IServerRegistrationRepository>();
                var servers = repo.GetAll().Where(x => x.IsActive).ToArray(); // fast, cached
                uow.Complete();
                return servers;
            }
        }

        /// <summary>
        /// Gets the local server identity.
        /// </summary>
        public string CurrentServerIdentity => CurrentServerIdentityValue;

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