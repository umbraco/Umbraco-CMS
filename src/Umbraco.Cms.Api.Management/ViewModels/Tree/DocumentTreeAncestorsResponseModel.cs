namespace Umbraco.Cms.Api.Management.ViewModels.Tree;

/// <summary>
/// Represents the ancestors of a document in the document tree.
/// </summary>
public class DocumentTreeAncestorsResponseModel
{
    /// <summary>
    /// Gets or sets the Id of the document the ancestors belong to.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the ancestors of the document, ordered from the root down. The document itself is not included.
    /// </summary>
    public IEnumerable<DocumentTreeItemResponseModel> Ancestors { get; set; } = [];
}
