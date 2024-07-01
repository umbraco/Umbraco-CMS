using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Routing;

[Authorize(Policy = AuthorizationPolicies.BackOfficeAccess)]
public class BackofficeHub : Hub
{
    public async Task SendMessage(string message) => await Clients.All.SendAsync("receiveMessage", message);
}
