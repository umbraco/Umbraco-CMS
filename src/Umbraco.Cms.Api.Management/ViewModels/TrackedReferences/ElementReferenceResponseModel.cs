using Umbraco.Cms.Api.Management.ViewModels.Element;

namespace Umbraco.Cms.Api.Management.ViewModels.TrackedReferences;

/// <summary>
/// Represents a response model containing information about a tracked element reference.
/// </summary>
public class ElementReferenceResponseModel : ReferenceResponseModel
{
    /// <summary>
    /// Gets or sets a value indicating whether the element is published.
    /// </summary>
    public bool? Published { get; set; }

    /// <summary>
    /// Gets or sets the document type information for the tracked reference.
    /// </summary>
    public TrackedReferenceDocumentType DocumentType { get; set; } = new();

    /// <summary>
    /// Gets or sets the collection of variant items associated with the element reference.
    /// </summary>
    public IEnumerable<ElementVariantItemResponseModel> Variants { get; set; } = Enumerable.Empty<ElementVariantItemResponseModel>();
}