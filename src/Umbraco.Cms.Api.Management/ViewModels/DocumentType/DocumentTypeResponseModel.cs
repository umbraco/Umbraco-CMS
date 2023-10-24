using Umbraco.Cms.Api.Common.Attributes;
using Umbraco.Cms.Api.Management.ViewModels.ContentType;

namespace Umbraco.Cms.Api.Management.ViewModels.DocumentType;

[ShortGenericSchemaName<DocumentTypePropertyTypeResponseModel, DocumentTypePropertyTypeContainerResponseModel>("ContentTypeForDocumentTypeResponse")]
public class DocumentTypeResponseModel : ContentTypeResponseModelBase<DocumentTypePropertyTypeResponseModel, DocumentTypePropertyTypeContainerResponseModel>
{
    public IEnumerable<Guid> AllowedTemplateIds { get; set; } = Array.Empty<Guid>();

    public Guid? DefaultTemplateId { get; set; }

    public ContentTypeCleanup Cleanup { get; set; } = new();
}
