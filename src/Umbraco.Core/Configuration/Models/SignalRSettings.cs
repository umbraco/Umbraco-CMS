using System.ComponentModel;

namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
/// Typed configuration options for SignalR settings.
/// </summary>
/// <remarks>
/// <para>
/// Server-side transport restrictions are applied to every Umbraco SignalR hub endpoint
/// via <c>HttpConnectionDispatcherOptions.Transports</c>. These settings are also forwarded to the
/// client via the server configuration endpoint, which is accessible via the
/// <c>/umbraco/management/api/v1/server/configuration</c> endpoint.
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
    /// Gets or sets the transport protocols the server will accept for SignalR connections.
    /// </summary>
    /// <remarks>
    /// When <c>null</c> (the default), all transports are accepted (framework default behavior).
    /// Set to <see cref="SignalRTransportType.WebSockets"/> to restrict connections to WebSockets
    /// only, which is required for load-balanced deployments without sticky sessions.
    /// </remarks>
    public SignalRTransportType? Transports { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the client should skip the SignalR negotiate
    /// round-trip and connect directly on one of the configured <see cref="Transports"/>.
    /// </summary>
    [DefaultValue(StaticClientShouldSkipNegotiation)]
    public bool ClientShouldSkipNegotiation { get; set; } = StaticClientShouldSkipNegotiation;
}
