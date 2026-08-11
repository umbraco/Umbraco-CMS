using Umbraco.Cms.Api.Management.ViewModels.Content;
using Umbraco.Cms.Api.Management.ViewModels.Element.Item;

namespace Umbraco.Cms.Api.Management.ViewModels.Element.References;

/// <summary>
/// Represents an element that is directly referenced by another entity and is not fully published.
/// </summary>
public class ReferencedElementWithPendingChangesResponseModel
{
    /// <summary>
    /// Gets or sets the referenced element.
    /// </summary>
    public required ElementItemResponseModel Element { get; set; }

    /// <summary>
    /// Gets or sets the aggregate ("worst") publish state across all variants of the element.
    /// </summary>
    public required PublishableVariantState State { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the element has a future scheduled publish date on any variant.
    /// </summary>
    public required bool IsScheduled { get; set; }
}
