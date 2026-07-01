namespace Umbraco.Cms.Api.Management.ViewModels.Server;

/// <summary>
/// Represents the base model for server configuration settings.
/// </summary>
public class ServerConfigurationBaseModel
{
    /// <summary>
    /// Gets or sets the collection of server configuration items represented by <see cref="ServerConfigurationItemResponseModel"/>.
    /// </summary>
    public IEnumerable<ServerConfigurationItemResponseModel> Items { get; set; } = Enumerable.Empty<ServerConfigurationItemResponseModel>();
}
