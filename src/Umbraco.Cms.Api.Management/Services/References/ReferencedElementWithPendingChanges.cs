using Umbraco.Cms.Api.Management.ViewModels.Content;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Api.Management.Services.References;

/// <summary>
/// Transports an element that is directly referenced by another entity and is not fully published, together with
/// the data needed to describe why, from <see cref="IReferencedElementsService"/> to the presentation layer.
/// </summary>
public sealed class ReferencedElementWithPendingChanges
{
    /// <summary>
    /// Gets the referenced element.
    /// </summary>
    public required IElementEntitySlim Element { get; init; }

    /// <summary>
    /// Gets the aggregate ("worst") publish state across all variants of the element.
    /// </summary>
    public required PublishableVariantState State { get; init; }

    /// <summary>
    /// Gets a value indicating whether the element has a future scheduled publish date on any variant.
    /// </summary>
    public required bool IsScheduled { get; init; }
}
