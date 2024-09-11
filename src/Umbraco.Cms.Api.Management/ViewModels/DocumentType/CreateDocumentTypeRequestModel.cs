using Umbraco.Cms.Api.Management.ViewModels.ContentType;

namespace Umbraco.Cms.Api.Management.ViewModels.DocumentType;

public class CreateDocumentTypeRequestModel
    : CreateContentTypeWithParentRequestModelBase<CreateDocumentTypePropertyTypeRequestModel, CreateDocumentTypePropertyTypeContainerRequestModel>
{
    public IEnumerable<ReferenceByIdModel> AllowedTemplates { get; set; } = Enumerable.Empty<ReferenceByIdModel>();

    public ReferenceByIdModel? DefaultTemplate { get; set; }

    public DocumentTypeCleanup Cleanup { get; set; } = new();

    public IEnumerable<DocumentTypeSort> AllowedDocumentTypes { get; set; } = Enumerable.Empty<DocumentTypeSort>();

    public IEnumerable<DocumentTypeComposition> Compositions { get; set; } = Enumerable.Empty<DocumentTypeComposition>();
}
