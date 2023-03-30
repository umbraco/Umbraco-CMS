namespace Umbraco.Cms.Api.Management.ViewModels.Item;

public class ItemResponseModelBase
{
    public string Name { get; set; } = string.Empty;

    public Guid Id { get; set; }

    public string Icon { get; set; } = string.Empty;
}
