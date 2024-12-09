using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Routing;

[Authorize(Policy = AuthorizationPolicies.BackOfficeAccess)]
public class ServerEventHub : Hub<IServerEventHub>
{
    private readonly IServerEventRouter _serverEventRouter;

    public ServerEventHub(IServerEventRouter serverEventRouter)
    {
        _serverEventRouter = serverEventRouter;
    }

    public override async Task OnConnectedAsync()
    {
        ClaimsPrincipal? principal = Context.User;

        if (principal is null)
        {
            Context.Abort();
            return;
        }

        await _serverEventRouter.AssignToGroups(principal, Context.ConnectionId);
    }

    public override Task OnDisconnectedAsync(Exception? exception) => base.OnDisconnectedAsync(exception);
}
