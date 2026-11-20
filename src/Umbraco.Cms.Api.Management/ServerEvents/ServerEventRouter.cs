using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models.ServerEvents;
using Umbraco.Cms.Core.ServerEvents;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.ServerEvents;

/// <inheritdoc />
/// <remarks>
/// Events broadcast to an event source group rely on the SignalR backplane to reach connections held by other servers.
/// Events delivered per connection are distributed to every server through the <see cref="ServerEventCacheRefresher"/>,
/// as each server only knows the connections it holds. <see cref="ServerEventCacheRefresherNotificationHandler"/> delivers them on each server.
/// </remarks>
internal sealed class ServerEventRouter : IServerEventRouter
{
    private readonly IHubContext<ServerEventHub, IServerEventHub> _eventHub;
    private readonly IRuntimeState _runtimeState;
    private readonly ILogger<ServerEventRouter> _logger;
    private readonly IServerEventAccessService _accessService;
    private readonly DistributedCache _distributedCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServerEventRouter"/> class.
    /// </summary>
    /// <param name="eventHub">The SignalR hub context used to send server events to connected clients.</param>
    /// <param name="runtimeState">Provides information about the current runtime state of the application.</param>
    /// <param name="logger">The logger used for logging events and errors related to the server event router.</param>
    /// <param name="accessService">Determines which event sources are filtered per recipient.</param>
    /// <param name="distributedCache">Distributes events that are delivered per connection to every server.</param>
    public ServerEventRouter(
        IHubContext<ServerEventHub, IServerEventHub> eventHub,
        IRuntimeState runtimeState,
        ILogger<ServerEventRouter> logger,
        IServerEventAccessService accessService,
        DistributedCache distributedCache)
    {
        _eventHub = eventHub;
        _runtimeState = runtimeState;
        _logger = logger;
        _accessService = accessService;
        _distributedCache = distributedCache;
    }

    /// <inheritdoc/>
    public async Task RouteEventAsync(ServerEvent serverEvent)
    {
        if (_runtimeState.Level != RuntimeLevel.Run)
        {
            return;
        }

        try
        {
            await _eventHub.Clients.Group(serverEvent.EventSource).notify(serverEvent);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to route server event {EventType} for {EventSource}", serverEvent.EventType, serverEvent.EventSource);
        }
    }

    /// <inheritdoc/>
    public async Task RouteEventAsync(ServerEvent serverEvent, ServerEventRoutingContext context)
    {
        if (_runtimeState.Level != RuntimeLevel.Run)
        {
            return;
        }

        // Sources with no registered access filter are broadcast to the whole source group.
        if (_accessService.AppliesTo(serverEvent.EventSource) is false)
        {
            await RouteEventAsync(serverEvent);
            return;
        }

        Distribute(
            serverEvent,
            new ServerEventCacheRefresher.JsonPayload
            {
                EventSource = serverEvent.EventSource,
                EventType = serverEvent.EventType,
                Key = serverEvent.Key,
                EntityPath = context.EntityPath,
            });
    }

    /// <inheritdoc/>
    public Task NotifyUserAsync(ServerEvent serverEvent, Guid userKey)
    {
        if (_runtimeState.Level != RuntimeLevel.Run)
        {
            return Task.CompletedTask;
        }

        Distribute(
            serverEvent,
            new ServerEventCacheRefresher.JsonPayload
            {
                EventSource = serverEvent.EventSource,
                EventType = serverEvent.EventType,
                Key = serverEvent.Key,
                UserKey = userKey,
            });

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public async Task BroadcastEventAsync(ServerEvent serverEvent)
    {
        if (_runtimeState.Level != RuntimeLevel.Run)
        {
            return;
        }

        try
        {
            await _eventHub.Clients.All.notify(serverEvent);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to broadcast server event {EventType}", serverEvent.EventType);
        }
    }

    private void Distribute(ServerEvent serverEvent, ServerEventCacheRefresher.JsonPayload payload)
    {
        try
        {
            _distributedCache.RefreshByPayload(ServerEventCacheRefresher.UniqueId, new[] { payload });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to distribute server event {EventType} for {EventSource}", serverEvent.EventType, serverEvent.EventSource);
        }
    }
}
