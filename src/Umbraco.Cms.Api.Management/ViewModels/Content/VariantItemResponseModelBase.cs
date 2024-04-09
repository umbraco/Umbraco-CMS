namespace Umbraco.Cms.Api.Management.ViewModels.Content;

public abstract class VariantItemResponseModelBase
{
    public required string Name { get; set; }

    public string? Culture { get; set; }
}
