using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Api.Management.ViewModels.Item;

namespace Umbraco.Cms.Api.Management.ViewModels.Element.Item;

/// <summary>
/// Represents a response model containing information about an element item returned by the Umbraco Management API.
/// </summary>
public class ElementItemResponseModel : ItemResponseModelBase
{
    /// <summary>
    /// Gets or sets a reference to the parent element of this item, if any.
    /// </summary>
    public ReferenceByIdModel? Parent { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the element has child items.
    /// </summary>
    public bool HasChildren { get; set; }

    /// <summary>
    /// Gets or sets a reference to the document type associated with this element.
    /// </summary>
    public DocumentTypeReferenceResponseModel DocumentType { get; set; } = new();

    /// <summary>
    /// Gets or sets the collection of variants for the element.
    /// Each variant represents a different language or segment of the element.
    /// </summary>
    public IEnumerable<ElementVariantItemResponseModel> Variants { get; set; } = Enumerable.Empty<ElementVariantItemResponseModel>();
}
