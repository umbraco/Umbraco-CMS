using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;

namespace Umbraco.Cms.Api.Management.ViewModels.Tree;

/// <summary>
/// Represents a response model for an item in the document blueprint tree.
/// </summary>
public class DocumentBlueprintTreeItemResponseModel : FolderTreeItemResponseModel
{
    /// <summary>
    /// Gets or sets the document type reference associated with the document blueprint.
    /// </summary>
    public DocumentTypeReferenceResponseModel? DocumentType { get; set; }

    /// <summary>
    /// Gets or sets the collection of variants for the item. Folders have a single invariant variant.
    /// </summary>
    public IEnumerable<DocumentVariantItemResponseModel> Variants { get; set; } = Enumerable.Empty<DocumentVariantItemResponseModel>();
}
