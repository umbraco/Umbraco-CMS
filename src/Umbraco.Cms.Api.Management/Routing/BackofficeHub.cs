using Microsoft.AspNetCore.SignalR;

namespace Umbraco.Cms.Api.Management.Routing;

public class BackofficeHub : Hub
{
    public async Task SendMessage(object payload) => await Clients.All.SendAsync("receiveMessage", payload);
}
