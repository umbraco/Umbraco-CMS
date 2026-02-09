namespace Umbraco.Cms.Api.Management.ViewModels.Server;

[Obsolete("Upgrade checks are no longer supported and this model will be removed in Umbraco 19.")]
public class UpgradeCheckResponseModel
{
    public required string Type { get; init; }

    public required string Comment { get; init; }

    public required string Url { get; init; }
}
