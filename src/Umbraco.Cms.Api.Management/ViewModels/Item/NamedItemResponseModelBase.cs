namespace Umbraco.Cms.Api.Management.ViewModels.Item;

/// <summary>
/// Represents the base response model for an item with a name.
/// </summary>
public abstract class NamedItemResponseModelBase : ItemResponseModelBase
{
    /// <summary>
    /// Gets or sets the name of the item.
    /// </summary>
    public string Name { get; set; } = string.Empty;
}
