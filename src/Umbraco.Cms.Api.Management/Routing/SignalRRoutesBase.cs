using Microsoft.AspNetCore.Http.Connections;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Api.Management.Routing;

/// <summary>
/// Base class for route definitions that map SignalR hub endpoints,
/// applying shared transport configuration from <see cref="SignalRSettings"/>.
/// </summary>
public class SignalRRoutesBase
{
    private readonly SignalRSettings _signalRSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="SignalRRoutesBase"/> class.
    /// </summary>
    /// <param name="signalRSettings">The SignalR settings options.</param>
    public SignalRRoutesBase(IOptions<SignalRSettings> signalRSettings)
    {
        _signalRSettings = signalRSettings.Value;
    }

    /// <summary>
    /// Configures the transport options for a SignalR hub endpoint.
    /// When <see cref="SignalRSettings.ClientShouldSkipNegotiation"/> is enabled,
    /// restricts the endpoint to WebSocket transport only so clients can skip the negotiate round-trip.
    /// </summary>
    /// <param name="options">The hub endpoint dispatcher options to configure.</param>
    protected void ConfigureHubEndpoint(HttpConnectionDispatcherOptions options)
    {
        if (_signalRSettings.ClientShouldSkipNegotiation)
        {
            options.Transports = HttpTransportType.WebSockets;
        }
    }
}
