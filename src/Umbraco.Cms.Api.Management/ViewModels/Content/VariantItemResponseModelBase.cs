namespace Umbraco.Cms.Api.Management.ViewModels.Content;

/// <summary>
/// Serves as the base class for response models representing content variant items.
/// </summary>
public abstract class VariantItemResponseModelBase
{
    /// <summary>
    /// Gets or sets the name of the variant item.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the culture code (e.g., "en-US") associated with this content variant.
    /// </summary>
    public string? Culture { get; set; }
}
