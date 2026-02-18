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

    [Obsolete("Please use the constructor that takes all parameters. Scheduled for removal in Umbraco 18.")]
    public ServerEventRouter(
        IHubContext<ServerEventHub, IServerEventHub> eventHub,
        IUserConnectionManager connectionManager)
        : this(
            eventHub,
            connectionManager,
            StaticServiceProvider.Instance.GetRequiredService<IRuntimeState>(),
            StaticServiceProvider.Instance.GetRequiredService<ILogger<ServerEventRouter>>())
    {
    }

    public ServerEventRouter(
        IHubContext<ServerEventHub, IServerEventHub> eventHub,
        IUserConnectionManager connectionManager,
        IRuntimeState runtimeState,
        ILogger<ServerEventRouter> logger)
    {
        _eventHub = eventHub;
        _connectionManager = connectionManager;
        _runtimeState = runtimeState;
        _logger = logger;
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
