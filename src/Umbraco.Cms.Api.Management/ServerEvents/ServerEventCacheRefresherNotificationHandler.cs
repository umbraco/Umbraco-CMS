using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.ServerEvents;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.ServerEvents;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Api.Management.ServerEvents;

/// <summary>
/// Delivers server events distributed through the <see cref="ServerEventCacheRefresher"/> to the connections held by this server.
/// </summary>
/// <remarks>
/// The handler runs on every server, so each one only sends to its own connections and no connection receives an event twice.
/// </remarks>
internal sealed class ServerEventCacheRefresherNotificationHandler : INotificationAsyncHandler<ServerEventCacheRefresherNotification>
{
    private readonly IHubContext<ServerEventHub, IServerEventHub> _eventHub;
    private readonly IUserConnectionManager _connectionManager;
    private readonly IServerEventAccessService _accessService;
    private readonly IRuntimeState _runtimeState;
    private readonly ILogger<ServerEventCacheRefresherNotificationHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServerEventCacheRefresherNotificationHandler"/> class.
    /// </summary>
    /// <param name="eventHub">The SignalR hub context used to send server events to connected clients.</param>
    /// <param name="connectionManager">Tracks the connections held by this server.</param>
    /// <param name="accessService">Resolves the connections authorized to receive an event for a filtered source.</param>
    /// <param name="runtimeState">Provides information about the current runtime state of the application.</param>
    /// <param name="logger">The logger.</param>
    public ServerEventCacheRefresherNotificationHandler(
        IHubContext<ServerEventHub, IServerEventHub> eventHub,
        IUserConnectionManager connectionManager,
        IServerEventAccessService accessService,
        IRuntimeState runtimeState,
        ILogger<ServerEventCacheRefresherNotificationHandler> logger)
    {
        _eventHub = eventHub;
        _connectionManager = connectionManager;
        _accessService = accessService;
        _runtimeState = runtimeState;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task HandleAsync(ServerEventCacheRefresherNotification notification, CancellationToken cancellationToken)
    {
        if (_runtimeState.Level != RuntimeLevel.Run
            || notification.MessageType != MessageType.RefreshByPayload
            || notification.MessageObject is not ServerEventCacheRefresher.JsonPayload[] payloads)
        {
            return;
        }

        foreach (ServerEventCacheRefresher.JsonPayload payload in payloads)
        {
            var serverEvent = new ServerEvent
            {
                EventSource = payload.EventSource,
                EventType = payload.EventType,
                Key = payload.Key,
            };

            try
            {
                IReadOnlyList<string> connections = payload.UserKey.HasValue
                    ? [.. _connectionManager.GetConnections(payload.UserKey.Value)]
                    : await _accessService.GetAuthorizedConnectionsAsync(
                        payload.EventSource,
                        new ServerEventRoutingContext { EntityPath = payload.EntityPath });

                if (connections.Count == 0)
                {
                    continue;
                }

                await _eventHub.Clients.Clients(connections).notify(serverEvent);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to deliver server event {EventType} for {EventSource}", serverEvent.EventType, serverEvent.EventSource);
            }
        }
    }
}
