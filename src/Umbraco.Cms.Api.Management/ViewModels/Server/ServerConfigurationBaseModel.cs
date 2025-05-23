namespace Umbraco.Cms.Api.Management.ViewModels.Server;

public class ServerConfigurationBaseModel
{
    public IEnumerable<ServerConfigurationItemResponseModel> Items { get; set; } = Enumerable.Empty<ServerConfigurationItemResponseModel>();
}
