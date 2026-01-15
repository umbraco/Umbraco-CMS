using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Umbraco.Cms.Core.ServerEvents;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.ServerEvents;

[Authorize(Policy = AuthorizationPolicies.BackOfficeAccess)]
public class ServerEventHub : Hub<IServerEventHub>
{
    private readonly IUserConnectionManager _userConnectionManager;
    private readonly IServerEventUserManager _serverEventUserManager;

    public ServerEventHub(
        IUserConnectionManager userConnectionManager,
        IServerEventUserManager serverEventUserManager)
    {
        _userConnectionManager = userConnectionManager;
        _serverEventUserManager = serverEventUserManager;
    }

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
