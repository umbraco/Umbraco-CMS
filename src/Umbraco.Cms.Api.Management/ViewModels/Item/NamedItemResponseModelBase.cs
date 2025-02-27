namespace Umbraco.Cms.Api.Management.ViewModels.Item;

public abstract class NamedItemResponseModelBase : ItemResponseModelBase
{
    public string Name { get; set; } = string.Empty;
}
