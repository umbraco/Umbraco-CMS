using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Sync;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services.Implement;

/// <summary>
///     Manages server registrations in the database.
/// </summary>
public sealed class ServerRegistrationService : RepositoryService, IServerRegistrationService
{
    private readonly IHostingEnvironment _hostingEnvironment;
    private readonly IServerRegistrationRepository _serverRegistrationRepository;

    private ServerRole _currentServerRole = ServerRole.Unknown;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ServerRegistrationService" /> class.
    /// </summary>
    public ServerRegistrationService(
        ICoreScopeProvider scopeProvider,
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
    ///     Touches a server to mark it as active; deactivate stale servers.
    /// </summary>
    /// <param name="serverAddress">The server URL.</param>
    /// <param name="staleTimeout">The time after which a server is considered stale.</param>
    public void TouchServer(string serverAddress, TimeSpan staleTimeout)
    {
        var serverIdentity = GetCurrentServerIdentity();
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            scope.WriteLock(Constants.Locks.Servers);

            _serverRegistrationRepository.ClearCache(); // ensure we have up-to-date cache

            IServerRegistration[]? regs = _serverRegistrationRepository.GetMany()?.ToArray();
            var hasSchedulingPublisher = regs?.Any(x => ((ServerRegistration)x).IsSchedulingPublisher);
            IServerRegistration? server =
                regs?.FirstOrDefault(x => x.ServerIdentity?.InvariantEquals(serverIdentity) ?? false);

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
            if (hasSchedulingPublisher == false)
            {
                server.IsSchedulingPublisher = true;
            }

            _serverRegistrationRepository.Save(server);
            _serverRegistrationRepository.DeactiveStaleServers(staleTimeout); // triggers a cache reload

            // reload - cheap, cached
            regs = _serverRegistrationRepository.GetMany().ToArray();

            // default role is single server, but if registrations contain more
            // than one active server, then role is scheduling publisher or subscriber
            _currentServerRole = regs.Count(x => x.IsActive) > 1
                ? server.IsSchedulingPublisher ? ServerRole.SchedulingPublisher : ServerRole.Subscriber
                : ServerRole.Single;

            scope.Complete();
        }
    }

    /// <summary>
    ///     Deactivates a server.
    /// </summary>
    /// <param name="serverIdentity">The server unique identity.</param>
    public void DeactiveServer(string serverIdentity)
    {
        // because the repository caches "all" and has queries disabled...
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            scope.WriteLock(Constants.Locks.Servers);

            _serverRegistrationRepository
                .ClearCache(); // ensure we have up-to-date cache // ensure we have up-to-date cache

            IServerRegistration? server = _serverRegistrationRepository.GetMany()
                ?.FirstOrDefault(x => x.ServerIdentity?.InvariantEquals(serverIdentity) ?? false);
            if (server == null)
            {
                return;
            }

            server.IsActive = server.IsSchedulingPublisher = false;
            _serverRegistrationRepository.Save(server); // will trigger a cache reload // will trigger a cache reload

            scope.Complete();
        }
    }

    /// <summary>
    ///     Deactivates stale servers.
    /// </summary>
    /// <param name="staleTimeout">The time after which a server is considered stale.</param>
    public void DeactiveStaleServers(TimeSpan staleTimeout)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            scope.WriteLock(Constants.Locks.Servers);
            _serverRegistrationRepository.DeactiveStaleServers(staleTimeout);
            scope.Complete();
        }
    }

    /// <summary>
    ///     Return all active servers.
    /// </summary>
    /// <param name="refresh">A value indicating whether to force-refresh the cache.</param>
    /// <returns>All active servers.</returns>
    /// <remarks>
    ///     By default this method will rely on the repository's cache, which is updated each
    ///     time the current server is touched, and the period depends on the configuration. Use the
    ///     <paramref name="refresh" /> parameter to force a cache refresh and reload active servers
    ///     from the database.
    /// </remarks>
    public IEnumerable<IServerRegistration>? GetActiveServers(bool refresh = false) =>
        GetServers(refresh).Where(x => x.IsActive);

    /// <summary>
    ///     Return all servers (active and inactive).
    /// </summary>
    /// <param name="refresh">A value indicating whether to force-refresh the cache.</param>
    /// <returns>All servers.</returns>
    /// <remarks>
    ///     By default this method will rely on the repository's cache, which is updated each
    ///     time the current server is touched, and the period depends on the configuration. Use the
    ///     <paramref name="refresh" /> parameter to force a cache refresh and reload all servers
    ///     from the database.
    /// </remarks>
    public IEnumerable<IServerRegistration> GetServers(bool refresh = false)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            scope.ReadLock(Constants.Locks.Servers);
            if (refresh)
            {
                _serverRegistrationRepository.ClearCache();
            }

            return _serverRegistrationRepository.GetMany().ToArray(); // fast, cached // fast, cached
        }
    }

    /// <summary>
    ///     Gets the role of the current server.
    /// </summary>
    /// <returns>The role of the current server.</returns>
    public ServerRole GetCurrentServerRole() => _currentServerRole;

    /// <summary>
    ///     Gets the local server identity.
    /// </summary>
    private string GetCurrentServerIdentity() => Environment.MachineName // eg DOMAIN\SERVER
                                                 + "/" + _hostingEnvironment.ApplicationId; // eg /LM/S3SVC/11/ROOT;
}
