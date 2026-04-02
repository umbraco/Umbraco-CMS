using Umbraco.Cms.Api.Management.ViewModels.ContentType;

namespace Umbraco.Cms.Api.Management.ViewModels.DocumentType;

/// <summary>
/// Represents a model used to submit data for updating an existing document type in the Umbraco CMS.
/// </summary>
public class UpdateDocumentTypeRequestModel
    : UpdateContentTypeRequestModelBase<UpdateDocumentTypePropertyTypeRequestModel, UpdateDocumentTypePropertyTypeContainerRequestModel>
{
    /// <summary>
    /// Gets or sets the collection of allowed templates for the document type.
    /// </summary>
    public IEnumerable<ReferenceByIdModel> AllowedTemplates { get; set; } = Enumerable.Empty<ReferenceByIdModel>();

    /// <summary>
    /// Gets or sets the default template reference for the document type.
    /// </summary>
    public ReferenceByIdModel? DefaultTemplate { get; set; }

    /// <summary>
    /// Gets or sets the cleanup settings for the document type.
    /// </summary>
    public DocumentTypeCleanup Cleanup { get; set; } = new();

    /// <summary>
    /// Gets or sets the collection of allowed document types with their sort order.
    /// </summary>
    public IEnumerable<DocumentTypeSort> AllowedDocumentTypes { get; set; } = Enumerable.Empty<DocumentTypeSort>();

    /// <summary>
    /// Gets or sets the document type compositions that are included in this document type.
    /// </summary>
    public IEnumerable<DocumentTypeComposition> Compositions { get; set; } = Enumerable.Empty<DocumentTypeComposition>();
}
