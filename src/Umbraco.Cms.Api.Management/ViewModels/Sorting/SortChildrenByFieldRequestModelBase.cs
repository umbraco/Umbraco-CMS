using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Api.Management.ViewModels.Sorting;

/// <summary>
/// Base request model for sorting the children of a node by a system field.
/// </summary>
public abstract class SortChildrenByFieldRequestModelBase
{
    /// <summary>
    /// Gets or sets the system field to sort the children by.
    /// The create and update dates are node-level (not culture-specific).
    /// </summary>
    public required ContentSortField Field { get; init; }

    /// <summary>
    /// Gets or sets the direction to sort in.
    /// </summary>
    public required Direction Direction { get; init; }
}
