using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models.ServerEvents;
using Umbraco.Cms.Core.ServerEvents;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.ServerEvents;

/// <inheritdoc />
internal sealed class ServerEventRouter : IServerEventRouter
{
    private readonly IHubContext<ServerEventHub, IServerEventHub> _eventHub;
    private readonly IUserConnectionManager _connectionManager;
    private readonly IRuntimeState _runtimeState;
    private readonly ILogger<ServerEventRouter> _logger;
    private readonly IServerEventEntityAccessService _entityAccessService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServerEventRouter"/> class.
    /// </summary>
    /// <param name="eventHub">The SignalR hub context used to send server events to connected clients.</param>
    /// <param name="connectionManager">Manages user connections for server events.</param>
    /// <param name="runtimeState">Provides information about the current runtime state of the application.</param>
    /// <param name="logger">The logger used for logging events and errors related to the server event router.</param>
    /// <param name="entityAccessService">Resolves recipients of entity-scoped events by their start-node access.</param>
    public ServerEventRouter(
        IHubContext<ServerEventHub, IServerEventHub> eventHub,
        IUserConnectionManager connectionManager,
        IRuntimeState runtimeState,
        ILogger<ServerEventRouter> logger,
        IServerEventEntityAccessService entityAccessService)
    {
        _eventHub = eventHub;
        _connectionManager = connectionManager;
        _runtimeState = runtimeState;
        _logger = logger;
        _entityAccessService = entityAccessService;
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

        // Sources without a per-entity access boundary are broadcast to the whole source group.
        if (_entityAccessService.AppliesTo(serverEvent.EventSource) is false)
        {
            await RouteEventAsync(serverEvent);
            return;
        }

        // Entity-scoped sources are delivered only to the connections the access service authorizes.
        // The registered filters decide what to gate on (and fail closed when they cannot), so the
        // router does not inspect the context itself.
        try
        {
            IReadOnlyList<string> connections =
                await _entityAccessService.GetAuthorizedConnectionsAsync(serverEvent.EventSource, context);

            if (connections.Count == 0)
            {
                return;
            }

            await _eventHub.Clients.Clients(connections).notify(serverEvent);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to route server event {EventType} for {EventSource}", serverEvent.EventType, serverEvent.EventSource);
        }
    }

    /// <inheritdoc/>
    public async Task NotifyUserAsync(ServerEvent serverEvent, Guid userKey)
    {
        if (_runtimeState.Level != RuntimeLevel.Run)
        {
            return;
        }

        ISet<string> userConnections = _connectionManager.GetConnections(userKey);

        if (userConnections.Any() is false)
        {
            return;
        }

        try
        {
            await _eventHub.Clients.Clients(userConnections).notify(serverEvent);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to notify user {UserKey} of server event {EventType}", userKey, serverEvent.EventType);
        }
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
}
