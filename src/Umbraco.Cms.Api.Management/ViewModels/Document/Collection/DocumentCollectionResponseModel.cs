using Umbraco.Cms.Api.Management.ViewModels.Content;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;

namespace Umbraco.Cms.Api.Management.ViewModels.Document.Collection;

/// <summary>
/// Represents a response model containing a collection of documents, typically used in the Umbraco CMS API management context.
/// This model may include the documents themselves along with related metadata such as pagination information.
/// </summary>
public class DocumentCollectionResponseModel : ContentCollectionResponseModelBase<DocumentValueResponseModel, DocumentVariantResponseModel>, IHasFlags, IIsProtected
{
    /// <summary>
    /// Gets or sets a reference to the document type associated with this document collection.
    /// </summary>
    public DocumentTypeCollectionReferenceResponseModel DocumentType { get; set; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether this document is in the trash.
    /// </summary>
    public bool IsTrashed { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the document is protected (e.g., access-restricted).
    /// </summary>
    public bool IsProtected { get; set; }

    /// <summary>
    /// Gets or sets the collection of ancestor references by their IDs.
    /// </summary>
    public IEnumerable<ReferenceByIdModel> Ancestors { get; set; } = [];

    /// <summary>
    /// Gets or sets the identifier of the user who last updated the document collection.
    /// </summary>
    public string? Updater { get; set; }
}
