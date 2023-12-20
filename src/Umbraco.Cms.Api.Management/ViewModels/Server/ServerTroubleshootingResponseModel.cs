namespace Umbraco.Cms.Api.Management.ViewModels.Server;

public class ServerTroubleshootingResponseModel
{
    public required IEnumerable<ServerTroubleshootingItemResponseModel> Items { get; set; }
}
