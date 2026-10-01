// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.Persistence;

namespace Umbraco.Cms.Core.Cache;

/// <summary>
///     Ensures that distributed cache events are setup and the <see cref="IServerMessenger" /> is initialized
/// </summary>
public sealed class DatabaseServerMessengerNotificationHandler :
    INotificationHandler<UmbracoApplicationStartingNotification>, INotificationHandler<UmbracoRequestEndNotification>
{
    private readonly IUmbracoDatabaseFactory _databaseFactory;
    private readonly ILogger<DatabaseServerMessengerNotificationHandler> _logger;
    private readonly IServerMessenger _messenger;
    private readonly IRuntimeState _runtimeState;
    private readonly IRepositoryCacheVersionService _repositoryCacheVersionService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DatabaseServerMessengerNotificationHandler" /> class.
    /// </summary>
    [Obsolete("Please use the constructor with all parameters. Scheduled for removal in Umbraco 19.")]
    public DatabaseServerMessengerNotificationHandler(
        IServerMessenger serverMessenger,
        IUmbracoDatabaseFactory databaseFactory,
        ILogger<DatabaseServerMessengerNotificationHandler> logger,
        IRuntimeState runtimeState)
        : this(
            serverMessenger,
            databaseFactory,
            logger,
            runtimeState,
            StaticServiceProvider.Instance.GetRequiredService<IRepositoryCacheVersionService>())
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="DatabaseServerMessengerNotificationHandler" /> class.
    /// </summary>
    public DatabaseServerMessengerNotificationHandler(
        IServerMessenger serverMessenger,
        IUmbracoDatabaseFactory databaseFactory,
        ILogger<DatabaseServerMessengerNotificationHandler> logger,
        IRuntimeState runtimeState,
        IRepositoryCacheVersionService repositoryCacheVersionService)
    {
        _databaseFactory = databaseFactory;
        _logger = logger;
        _messenger = serverMessenger;
        _runtimeState = runtimeState;
        _repositoryCacheVersionService = repositoryCacheVersionService;
    }

    /// <inheritdoc />
    public void Handle(UmbracoApplicationStartingNotification notification)
    {
        if (_runtimeState.Level != RuntimeLevel.Run)
        {
            return;
        }

        if (_databaseFactory.CanConnect == false)
        {
            _logger.LogWarning(
                "Cannot connect to the database, distributed calls will not be enabled for this server.");
            return;
        }

        // Sync on startup, this will run through the messenger's initialization sequence
        _messenger?.Sync();
    }

    /// <summary>
    /// Handles the end of an Umbraco request: writes the batched distributed cache instructions, then publishes the
    /// repository cache versions deferred during the request.
    /// </summary>
    /// <param name="notification">The notification instance signaling the end of an Umbraco request.</param>
    /// <remarks>
    /// The order matters. A server that sees a new cache version before the matching instruction syncs, finds
    /// nothing to refresh and keeps its stale entries until the next periodic sync.
    /// </remarks>
    public void Handle(UmbracoRequestEndNotification notification)
    {
        _messenger?.SendMessages();
        _repositoryCacheVersionService.FlushCacheUpdatesAsync().GetAwaiter().GetResult();
    }
}
