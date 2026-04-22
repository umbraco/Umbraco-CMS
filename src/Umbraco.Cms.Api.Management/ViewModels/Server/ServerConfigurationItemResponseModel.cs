namespace Umbraco.Cms.Api.Management.ViewModels.Server;

/// <summary>
/// Represents the response model for a server configuration item returned by the management API.
/// </summary>
public class ServerConfigurationItemResponseModel
{
    /// <summary>
    /// Gets or sets the name of the server configuration item.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the configuration value for the server configuration item.
    /// </summary>
    public required string Data { get; set; }
}
