using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using Umbraco.Cms.Core.ServerEvents;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.ServerEvents;

/// <inheritdoc />
internal sealed class ServerEventUserManager : IServerEventUserManager
{
    private readonly IUserConnectionManager _userConnectionManager;
    private readonly EventSourceAuthorizerCollection _eventSourceAuthorizerCollection;
    private readonly IHubContext<ServerEventHub, IServerEventHub> _eventHub;

    public ServerEventUserManager(
        IUserConnectionManager userConnectionManager,
        EventSourceAuthorizerCollection eventSourceAuthorizerCollection,
        IHubContext<ServerEventHub, IServerEventHub> eventHub)
    {
        _userConnectionManager = userConnectionManager;
        _eventSourceAuthorizerCollection = eventSourceAuthorizerCollection;
        _eventHub = eventHub;
    }

    /// <inheritdoc />
    public async Task AssignToGroupsAsync(ClaimsPrincipal user, string connectionId)
    {
        foreach (IEventSourceAuthorizer authorizer in _eventSourceAuthorizerCollection)
        {
            foreach (var eventSource in authorizer.AuthorizableEventSources)
            {
                var isAuthorized = await authorizer.AuthorizeAsync(user, eventSource);
                if (isAuthorized)
                {
                    await _eventHub.Groups.AddToGroupAsync(connectionId, eventSource);
                }
            }
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

        foreach (IEventSourceAuthorizer authorizer in _eventSourceAuthorizerCollection)
        {
            foreach (var eventSource in authorizer.AuthorizableEventSources)
            {
                var isAuthorized = await authorizer.AuthorizeAsync(user, eventSource);

                if (isAuthorized)
                {
                    foreach (var connection in connections)
                    {
                        await _eventHub.Groups.AddToGroupAsync(connection, eventSource);
                    }

                    continue;
                }

                foreach (var connection in connections)
                {
                    await _eventHub.Groups.RemoveFromGroupAsync(connection, eventSource);
                }
            }
        }
    }
}
