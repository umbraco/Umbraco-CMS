using System.ComponentModel;

namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
/// Client-side SignalR settings that are forwarded to the backoffice frontend
/// via the server configuration endpoint.
/// </summary>
public class SignalRClientSettings
{
    internal const bool StaticSkipNegotiation = false;

    /// <summary>
    /// Gets or sets a value indicating whether the client should skip the SignalR negotiate
    /// round-trip and connect directly via WebSockets.
    /// </summary>
    /// <remarks>
    /// When set to <c>true</c> the client will set both <c>skipNegotiation</c> and
    /// <c>transport: WebSockets</c> on the hub connection, which avoids the negotiate HTTP
    /// request that causes failures in load-balanced deployments without sticky sessions.
    /// This is safe for self-hosted SignalR but must <b>not</b> be used with Azure SignalR Service.
    /// </remarks>
    [DefaultValue(StaticSkipNegotiation)]
    public bool SkipNegotiation { get; set; } = StaticSkipNegotiation;
}
