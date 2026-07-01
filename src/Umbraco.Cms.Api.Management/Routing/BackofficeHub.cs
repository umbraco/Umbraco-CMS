using Microsoft.AspNetCore.SignalR;

namespace Umbraco.Cms.Api.Management.Routing;

/// <summary>
/// Provides a SignalR hub for managing real-time communication and routing events within the Umbraco CMS backoffice API.
/// </summary>
public class BackofficeHub : Hub
{
    /// <summary>
    /// Sends the specified payload to all connected clients.
    /// </summary>
    /// <param name="payload">The payload object to send.</param>
    /// <returns>A task that represents the asynchronous send operation.</returns>
    public async Task SendPayload(object payload) => await Clients.All.SendAsync("payloadReceived", payload);
}
