using Microsoft.AspNetCore.SignalR;
using Umbraco.Cms.Core.Models.ServerEvents;
using Umbraco.Cms.Core.ServerEvents;

namespace Umbraco.Cms.Api.Management.ServerEvents;

/// <inheritdoc />
internal sealed class ServerEventRouter : IServerEventRouter
{
    private readonly IHubContext<ServerEventHub, IServerEventHub> _eventHub;

    public ServerEventRouter(
        IHubContext<ServerEventHub, IServerEventHub> eventHub) =>
        _eventHub = eventHub;

    /// <inheritdoc/>
    public Task RouteEventAsync(ServerEvent serverEvent)
        => _eventHub.Clients.Group(serverEvent.EventSource).notify(serverEvent);
}
