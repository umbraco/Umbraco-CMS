namespace Umbraco.Cms.Api.Management.ViewModels.Server;

public class ServerInformationResponseModel
{
    public required IEnumerable<ServerInformationItemResponseModel> Items { get; set; }
}
