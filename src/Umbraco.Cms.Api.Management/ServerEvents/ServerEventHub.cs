using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Umbraco.Cms.Core.ServerEvents;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.ServerEvents;

/// <summary>
/// A SignalR hub that facilitates the broadcasting and management of server events within the Umbraco CMS Management API.
/// </summary>
[Authorize(Policy = AuthorizationPolicies.BackOfficeAccess)]
public class ServerEventHub : Hub<IServerEventHub>
{
    private readonly IUserConnectionManager _userConnectionManager;
    private readonly IServerEventUserManager _serverEventUserManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.ServerEvents.ServerEventHub"/> class.
    /// </summary>
    /// <param name="userConnectionManager">Manages user connections for the server event hub.</param>
    /// <param name="serverEventUserManager">Handles user-related server event operations for the hub.</param>
    public ServerEventHub(
        IUserConnectionManager userConnectionManager,
        IServerEventUserManager serverEventUserManager)
    {
        _userConnectionManager = userConnectionManager;
        _serverEventUserManager = serverEventUserManager;
    }

    /// <summary>
    /// Handles a new client connection to the server event hub.
    /// Validates the connecting user, aborts the connection if validation fails, and assigns the connection to the appropriate user groups if successful.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation for handling the connection.</returns>
    public override async Task OnConnectedAsync()
    {
        ClaimsPrincipal? principal = Context.User;
        Guid? userKey = principal?.Identity?.GetUserKey();

        if (userKey is null)
        {
            Context.Abort();
            return;
        }

        _userConnectionManager.AddConnection(userKey.Value, Context.ConnectionId);
        await _serverEventUserManager.AssignToGroupsAsync(principal!, Context.ConnectionId);
    }

    /// <summary>
    /// Handles logic when a client connection is disconnected from the server event hub.
    /// If a user key is associated with the connection, removes the user's connection from the connection manager.
    /// </summary>
    /// <param name="exception">The exception that occurred during disconnection, if any; otherwise, <c>null</c>.</param>
    /// <returns>A completed <see cref="Task"/> representing the asynchronous operation.</returns>
    public override Task OnDisconnectedAsync(Exception? exception)
    {
        ClaimsPrincipal? principal = Context.User;
        Guid? userKey = principal?.Identity?.GetUserKey();

        if (userKey is null)
        {
            return Task.CompletedTask;
        }

        _userConnectionManager.RemoveConnection(userKey.Value, Context.ConnectionId);
        return Task.CompletedTask;
    }
}
