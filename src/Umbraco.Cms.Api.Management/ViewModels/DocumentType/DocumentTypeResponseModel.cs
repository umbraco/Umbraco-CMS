using Umbraco.Cms.Api.Management.ViewModels.ContentType;

namespace Umbraco.Cms.Api.Management.ViewModels.DocumentType;

/// <summary>
/// Represents a response model containing information about a document type, used in the management API.
/// </summary>
public class DocumentTypeResponseModel : ContentTypeResponseModelBase<DocumentTypePropertyTypeResponseModel, DocumentTypePropertyTypeContainerResponseModel>
{
    /// <summary>
    /// Gets or sets the collection of allowed templates for the document type.
    /// </summary>
    public IEnumerable<ReferenceByIdModel> AllowedTemplates { get; set; } = Enumerable.Empty<ReferenceByIdModel>();

    /// <summary>
    /// Gets or sets the default template associated with the document type.
    /// </summary>
    public ReferenceByIdModel? DefaultTemplate { get; set; }

    /// <summary>
    /// Gets or sets the cleanup configuration associated with this document type.
    /// </summary>
    public DocumentTypeCleanup Cleanup { get; set; } = new();

    /// <summary>
    /// Gets or sets the collection of document types that are allowed as children of this document type, including their sort order.
    /// </summary>
    public IEnumerable<DocumentTypeSort> AllowedDocumentTypes { get; set; } = Enumerable.Empty<DocumentTypeSort>();

    /// <summary>
    /// Gets or sets the compositions associated with the document type.
    /// </summary>
    public IEnumerable<DocumentTypeComposition> Compositions { get; set; } = Enumerable.Empty<DocumentTypeComposition>();
}
