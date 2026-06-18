using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Api.Management.ViewModels.Sorting;

/// <summary>
/// Request model for sorting the children of a document by a system field.
/// </summary>
public class SortDocumentChildrenByFieldRequestModel : SortChildrenByFieldRequestModelBase
{
    /// <summary>
    /// Gets or sets the culture whose variant name to sort by, or <c>null</c> to sort by the invariant name.
    /// Only applies when sorting by <see cref="ContentSortField.Name"/>. The culture is not validated: a document that
    /// does not vary by the given culture - or an unrecognised culture - falls back to the invariant name.
    /// </summary>
    public string? Culture { get; init; }
}
