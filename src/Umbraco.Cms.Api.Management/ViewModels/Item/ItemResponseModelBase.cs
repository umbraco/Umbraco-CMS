namespace Umbraco.Cms.Api.Management.ViewModels.Item;

public abstract class ItemResponseModelBase
{
    public string Name { get; set; } = string.Empty;

    public Guid Id { get; set; }
}
