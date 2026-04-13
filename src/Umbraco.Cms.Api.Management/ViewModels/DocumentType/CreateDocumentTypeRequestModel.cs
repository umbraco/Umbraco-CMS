using Umbraco.Cms.Api.Management.ViewModels.ContentType;

namespace Umbraco.Cms.Api.Management.ViewModels.DocumentType;

/// <summary>
/// Represents the data required to create a new document type in the Umbraco CMS.
/// </summary>
public class CreateDocumentTypeRequestModel
    : CreateContentTypeWithParentRequestModelBase<CreateDocumentTypePropertyTypeRequestModel, CreateDocumentTypePropertyTypeContainerRequestModel>
{
    /// <summary>
    /// Gets or sets the collection of allowed templates for the document type.
    /// </summary>
    public IEnumerable<ReferenceByIdModel> AllowedTemplates { get; set; } = Enumerable.Empty<ReferenceByIdModel>();

    /// <summary>
    /// Gets or sets a reference to the default template for the document type, identified by its ID.
    /// </summary>
    public ReferenceByIdModel? DefaultTemplate { get; set; }

    /// <summary>
    /// Gets or sets the cleanup configuration for this document type, specifying how obsolete or unused items are handled.
    /// </summary>
    public DocumentTypeCleanup Cleanup { get; set; } = new();

    /// <summary>
    /// Gets or sets the collection of document types that are allowed as children of this document type.
    /// </summary>
    public IEnumerable<DocumentTypeSort> AllowedDocumentTypes { get; set; } = Enumerable.Empty<DocumentTypeSort>();

    /// <summary>
    /// Gets or sets the document type compositions associated with this document type.
    /// This is a collection of <see cref="DocumentTypeComposition"/> instances.
    /// </summary>
    public IEnumerable<DocumentTypeComposition> Compositions { get; set; } = Enumerable.Empty<DocumentTypeComposition>();
}
