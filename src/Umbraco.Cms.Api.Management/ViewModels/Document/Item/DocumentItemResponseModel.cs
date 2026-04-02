using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Api.Management.ViewModels.Item;

namespace Umbraco.Cms.Api.Management.ViewModels.Document.Item;

/// <summary>
/// Represents a response model containing information about a document item returned by the Umbraco Management API.
/// </summary>
public class DocumentItemResponseModel : ItemResponseModelBase, IIsProtected
{
    /// <summary>
    /// Gets or sets a value indicating whether the document is trashed.
    /// </summary>
    public bool IsTrashed { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the document is protected.
    /// </summary>
    public bool IsProtected { get; set; }

    /// <summary>
    /// Gets or sets a reference to the parent document of this item, if any.
    /// </summary>
    public ReferenceByIdModel? Parent { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the document has child items.
    /// </summary>
    public bool HasChildren { get; set; }

    /// <summary>
    /// Gets or sets a reference to the document type associated with this document.
    /// </summary>
    public DocumentTypeReferenceResponseModel DocumentType { get; set; } = new();

    /// <summary>
    /// Gets or sets the collection of variants for the document.
    /// Each variant represents a different language or segment of the document.
    /// </summary>
    public IEnumerable<DocumentVariantItemResponseModel> Variants { get; set; } = Enumerable.Empty<DocumentVariantItemResponseModel>();
}
