namespace Umbraco.Cms.Api.Management.ViewModels.Server;

/// <summary>
/// Represents client-side SignalR settings returned by the server configuration endpoint.
/// </summary>
public class SignalRClientSettingsResponseModel
{
    /// <summary>Gets or sets a value indicating whether the client should skip the SignalR negotiate round-trip.</summary>
    public bool SkipNegotiation { get; set; }
}
