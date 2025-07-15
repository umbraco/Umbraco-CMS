using Microsoft.AspNetCore.SignalR;
using Umbraco.Cms.Core.Models.ServerEvents;
using Umbraco.Cms.Core.ServerEvents;

namespace Umbraco.Cms.Api.Management.ServerEvents;

/// <inheritdoc />
internal sealed class ServerEventRouter : IServerEventRouter
{
    private readonly IHubContext<ServerEventHub, IServerEventHub> _eventHub;
    private readonly IUserConnectionManager _connectionManager;

    public ServerEventRouter(
        IHubContext<ServerEventHub, IServerEventHub> eventHub,
        IUserConnectionManager connectionManager)
    {
        _eventHub = eventHub;
        _connectionManager = connectionManager;
    }

    /// <inheritdoc/>
    public Task RouteEventAsync(ServerEvent serverEvent)
        => _eventHub.Clients.Group(serverEvent.EventSource).notify(serverEvent);

    /// <inheritdoc/>
    public async Task NotifyUserAsync(ServerEvent serverEvent, Guid userKey)
    {
        ISet<string> userConnections = _connectionManager.GetConnections(userKey);

        if (userConnections.Any() is false)
        {
            return;
        }

        await _eventHub.Clients.Clients(userConnections).notify(serverEvent);
    }


    /// <inheritdoc/>
    public async Task BroadcastEventAsync(ServerEvent serverEvent) => await _eventHub.Clients.All.notify(serverEvent);
}
