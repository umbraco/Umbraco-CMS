using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Services
{

    /// <summary>
    /// Manages server registrations in the database.
    /// </summary>
    public sealed class ServerRegistrationService : RepositoryService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServerRegistrationService"/> class.
        /// </summary>
        /// <param name="uowProvider">A UnitOfWork provider.</param>
        /// <param name="repositoryFactory">A repository factory.</param>
        /// <param name="logger">A logger.</param>
        public ServerRegistrationService(IDatabaseUnitOfWorkProvider uowProvider, RepositoryFactory repositoryFactory, ILogger logger)
            : base(uowProvider, repositoryFactory, logger)
        { }

        /// <summary>
        /// Touches a server to mark it as active; deactivate stale servers.
        /// </summary>
        /// <param name="serverAddress">The server url.</param>
        /// <param name="serverIdentity">The server unique identity.</param>
        /// <param name="staleTimeout">The time after which a server is considered stale.</param>
        public void TouchServer(string serverAddress, string serverIdentity, TimeSpan staleTimeout)
        {
            var uow = UowProvider.GetUnitOfWork();
            using (var repo = RepositoryFactory.CreateServerRegistrationRepository(uow))
            {
                var query = Query<ServerRegistration>.Builder.Where(x => x.ServerIdentity.ToUpper() == serverIdentity.ToUpper());
                var server = repo.GetByQuery(query).FirstOrDefault();
                if (server == null)
                {
                    server = new ServerRegistration(serverAddress, serverIdentity, DateTime.UtcNow)
                    {
                        IsActive = true
                    };                    
                }
                else
                {
                    server.ServerAddress = serverAddress; // should not really change but it might!
                    server.UpdateDate = DateTime.UtcNow; // stick with Utc dates since these might be globally distributed
                    server.IsActive = true;
                }
                repo.AddOrUpdate(server);
                uow.Commit();

                repo.DeactiveStaleServers(staleTimeout);
            }
        }

        /// <summary>
        /// Deactivates a server.
        /// </summary>
        /// <param name="serverIdentity">The server unique identity.</param>
        public void DeactiveServer(string serverIdentity)
        {
            var uow = UowProvider.GetUnitOfWork();
            using (var repo = RepositoryFactory.CreateServerRegistrationRepository(uow))
            {
                var query = Query<ServerRegistration>.Builder.Where(x => x.ServerIdentity.ToUpper() == serverIdentity.ToUpper());
                var server = repo.GetByQuery(query).FirstOrDefault();
                if (server != null)
                {
                    server.IsActive = false;
                    repo.AddOrUpdate(server);
                    uow.Commit();
                }
            }
        }

        /// <summary>
        /// Deactivates stale servers.
        /// </summary>
        /// <param name="staleTimeout">The time after which a server is considered stale.</param>
        public void DeactiveStaleServers(TimeSpan staleTimeout)
        {
            var uow = UowProvider.GetUnitOfWork();
            using (var repo = RepositoryFactory.CreateServerRegistrationRepository(uow))
            {
                repo.DeactiveStaleServers(staleTimeout);
            }
        }

        /// <summary>
        /// Return all active servers.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ServerRegistration> GetActiveServers()
        {
            var uow = UowProvider.GetUnitOfWork();
            using (var repo = RepositoryFactory.CreateServerRegistrationRepository(uow))
            {
                var query = Query<ServerRegistration>.Builder.Where(x => x.IsActive);
                return repo.GetByQuery(query).ToArray();
            }
        }
    }
}