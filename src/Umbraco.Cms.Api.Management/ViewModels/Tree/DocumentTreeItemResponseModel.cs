using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;

namespace Umbraco.Cms.Api.Management.ViewModels.Tree;

/// <summary>
/// Represents a response model for an item within the document tree in the Umbraco CMS Management API.
/// Used to convey information about a single document node in hierarchical content structures.
/// </summary>
public class DocumentTreeItemResponseModel : ContentTreeItemResponseModel, IIsProtected
{
    /// <summary>
    /// Gets or sets a value indicating whether the document is protected by access restrictions.
    /// </summary>
    public bool IsProtected { get; set; }

    /// <summary>
    /// Gets or sets the collection of ancestor nodes referenced by their IDs.
    /// </summary>
    public IEnumerable<ReferenceByIdModel> Ancestors { get; set; } = [];

    /// <summary>
    /// Gets or sets a reference to the document type that defines the structure of this tree item.
    /// </summary>
    public DocumentTypeReferenceResponseModel DocumentType { get; set; } = new();

    /// <summary>
    /// Gets or sets the collection of variants for the document, where each variant represents a specific culture or segment.
    /// </summary>
    public IEnumerable<DocumentVariantItemResponseModel> Variants { get; set; } = Enumerable.Empty<DocumentVariantItemResponseModel>();
}
