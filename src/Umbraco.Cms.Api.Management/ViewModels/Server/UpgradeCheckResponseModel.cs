namespace Umbraco.Cms.Api.Management.ViewModels.Server;

public class UpgradeCheckResponseModel
{
    public required string Type { get; init; }

    public required string Comment { get; init; }

    public required string Url { get; init; }
}
