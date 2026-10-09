using System.ComponentModel;

namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
/// Typed configuration options for SignalR settings.
/// </summary>
/// <remarks>
/// <para>
/// When <see cref="ClientShouldSkipNegotiation"/> is enabled, all hub endpoints are restricted
/// to WebSocket transport and the client skips the negotiate round-trip. The setting is forwarded
/// to the client via the <c>/umbraco/management/api/v1/server/configuration</c> endpoint.
/// </para>
/// <para>
/// Downstream packages (e.g. Umbraco Cloud) can configure these settings via
/// <c>IConfigureOptions&lt;SignalRSettings&gt;</c> or <c>appsettings.json</c>
/// under <c>Umbraco:CMS:SignalR</c>.
/// </para>
/// </remarks>
[UmbracoOptions(Constants.Configuration.ConfigSignalR)]
public class SignalRSettings
{
    internal const bool StaticClientShouldSkipNegotiation = false;

    /// <summary>
    /// Gets or sets a value indicating whether the client should skip the SignalR negotiate
    /// round-trip and connect directly via WebSockets.
    /// </summary>
    /// <remarks>
    /// When <c>true</c>, the server restricts all hub endpoints to the WebSocket transport only
    /// (via <c>HttpConnectionDispatcherOptions.Transports</c>) and the client is instructed to
    /// set <c>skipNegotiation = true</c> with <c>transport = WebSockets</c>. This eliminates the
    /// negotiate HTTP request that causes failures in load-balanced deployments without sticky sessions.
    /// This is safe for self-hosted SignalR but must <b>not</b> be used with Azure SignalR Service.
    /// </remarks>
    [DefaultValue(StaticClientShouldSkipNegotiation)]
    public bool ClientShouldSkipNegotiation { get; set; } = StaticClientShouldSkipNegotiation;
}
