using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using Umbraco.Cms.Core.Models.ServerEvents;
using Umbraco.Cms.Core.ServerEvents;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.ServerEvents;

/// <inheritdoc />
internal sealed class ServerEventUserManager : IServerEventUserManager
{
    private readonly IUserConnectionManager _userConnectionManager;
    private readonly IServerEventAuthorizationService _serverEventAuthorizationService;
    private readonly IHubContext<ServerEventHub, IServerEventHub> _eventHub;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServerEventUserManager"/> class.
    /// </summary>
    /// <param name="userConnectionManager">Manages user connections for server events.</param>
    /// <param name="serverEventAuthorizationService">Service used to authorize server event actions for users.</param>
    /// <param name="eventHub">The SignalR hub context for <see cref="ServerEventHub"/>, used to communicate with connected clients.</param>
    public ServerEventUserManager(
        IUserConnectionManager userConnectionManager,
        IServerEventAuthorizationService serverEventAuthorizationService,
        IHubContext<ServerEventHub, IServerEventHub> eventHub)
    {
        _userConnectionManager = userConnectionManager;
        _serverEventAuthorizationService = serverEventAuthorizationService;
        _eventHub = eventHub;
    }

    /// <inheritdoc />
    public async Task AssignToGroupsAsync(ClaimsPrincipal user, string connectionId)
    {
        SeverEventAuthorizationResult authorizationResult = await _serverEventAuthorizationService.AuthorizeAsync(user);

        foreach (var authorizedEventSource in authorizationResult.AuthorizedEventSources)
        {
           await _eventHub.Groups.AddToGroupAsync(connectionId, authorizedEventSource);
        }
    }

    /// <inheritdoc />
    public async Task RefreshGroupsAsync(ClaimsPrincipal user)
    {
        Guid? userKey = user.Identity?.GetUserKey();

        // If we can't resolve the user key from the principal something is quite wrong, and we shouldn't continue.
        if (userKey is null)
        {
            throw new InvalidOperationException("Unable to resolve user key.");
        }

        // Ensure that all the users connections are removed from all groups.
        ISet<string> connections = _userConnectionManager.GetConnections(userKey.Value);

        // If there's no connection there's nothing to do.
        if (connections.Count == 0)
        {
            return;
        }

        SeverEventAuthorizationResult authorizationResult = await _serverEventAuthorizationService.AuthorizeAsync(user);

        // Add the user to the authorized groups, and remove them from the unauthorized groups.
        // Note that it's safe to add a user to a group multiple times, so we don't have ot worry about that.
        foreach (var authorizedEventSource in authorizationResult.AuthorizedEventSources)
        {
            foreach (var connection in connections)
            {
                await _eventHub.Groups.AddToGroupAsync(connection, authorizedEventSource);
            }
        }

        foreach (var unauthorizedEventSource in authorizationResult.UnauthorizedEventSources)
        {
            foreach (var connection in connections)
            {
                await _eventHub.Groups.RemoveFromGroupAsync(connection, unauthorizedEventSource);
            }
        }
    }
}
