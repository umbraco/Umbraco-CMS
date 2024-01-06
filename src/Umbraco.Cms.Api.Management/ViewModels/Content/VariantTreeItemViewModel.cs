namespace Umbraco.Cms.Api.Management.ViewModels.Content;

public class VariantItemResponseModel
{
    public required string Name { get; set; }

    public string? Culture { get; set; }

    public required ContentState State { get; set; }
}
