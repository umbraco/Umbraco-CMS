namespace Umbraco.Cms.Api.Management.ViewModels.Item;

public abstract class NamedItemResponseModelBase : ItemResponseModelBase
{
    public string Name { get; set; } = string.Empty;
}

public abstract class ItemResponseModelBase
{
    public Guid Id { get; set; }
}
