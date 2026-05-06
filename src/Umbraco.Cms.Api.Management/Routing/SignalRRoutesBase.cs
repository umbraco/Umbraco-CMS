using Microsoft.AspNetCore.Http.Connections;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Routing;

/// <summary>
/// Base class for route definitions that map SignalR hub endpoints,
/// applying shared transport configuration from <see cref="SignalRSettings"/>.
/// </summary>
public abstract class SignalRRoutesBase
{
    private readonly SignalRSettings _signalRSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="SignalRRoutesBase"/> class.
    /// </summary>
    /// <param name="runtimeState">The current runtime state of the Umbraco application.</param>
    /// <param name="signalRSettings">The SignalR settings options.</param>
    protected SignalRRoutesBase(IRuntimeState runtimeState, IOptions<SignalRSettings> signalRSettings)
    {
        RuntimeState = runtimeState;
        _signalRSettings = signalRSettings.Value;
    }

    /// <summary>
    /// Gets the current runtime state of the Umbraco application.
    /// </summary>
    protected IRuntimeState RuntimeState { get; }

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
