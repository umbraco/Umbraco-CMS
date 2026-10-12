using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Routing;

/// <summary>
/// Provides a SignalR hub for managing real-time communication and routing events within the Umbraco CMS backoffice API.
/// </summary>
[Authorize(Policy = AuthorizationPolicies.BackOfficeAccess)]
public class BackofficeHub : Hub
{
    /// <summary>
    /// Sends the specified payload to all connected clients.
    /// </summary>
    /// <param name="payload">The payload object to send.</param>
    /// <returns>A task that represents the asynchronous send operation.</returns>
    public async Task SendPayload(object payload) => await Clients.All.SendAsync("payloadReceived", payload);
}
