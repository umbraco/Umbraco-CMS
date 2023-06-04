using Umbraco.Cms.Api.Management.ViewModels.ContentType;

namespace Umbraco.Cms.Api.Management.ViewModels.DocumentType;

public class CreateDocumentTypeRequestModel
    : CreateContentTypeRequestModelBase<CreateDocumentTypePropertyTypeRequestModel, CreateDocumentTypePropertyTypeContainerRequestModel>, IDocumentTypeRequestModel
{
    public IEnumerable<Guid> AllowedTemplateIds { get; set; } = Array.Empty<Guid>();

    public Guid? DefaultTemplateId { get; set; }

    public ContentTypeCleanup Cleanup { get; set; } = new();
}
