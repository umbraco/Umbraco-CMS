using Microsoft.AspNetCore.SignalR;

namespace Umbraco.Cms.Api.Management.Routing;

public class BackofficeHub : Hub
{
    public async Task SendPayload(object payload) => await Clients.All.SendAsync("payloadReceived", payload);
}
