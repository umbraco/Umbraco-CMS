using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Api.Management.ViewModels.Server;

/// <summary>
/// Represents client-side SignalR settings returned by the server configuration endpoint.
/// </summary>
public class SignalRClientSettingsResponseModel
{
    /// <summary>Gets or sets a value indicating whether the client should skip the SignalR negotiate round-trip.</summary>
    public bool SkipNegotiation { get; set; }

    /// <summary>
    /// Gets or sets the transport protocols the server will accept for SignalR connections.
    /// </summary>
    public SignalRTransportType? Transports { get; set; }
}
