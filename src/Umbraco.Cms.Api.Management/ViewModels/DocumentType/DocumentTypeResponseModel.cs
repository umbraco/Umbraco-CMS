using Umbraco.Cms.Api.Common.Attributes;
using Umbraco.Cms.Api.Management.ViewModels.ContentType;

namespace Umbraco.Cms.Api.Management.ViewModels.DocumentType;

[ShortGenericSchemaName<DocumentTypePropertyTypeResponseModel, DocumentTypePropertyTypeContainerResponseModel>("ContentTypeForDocumentTypeResponseModel")]
public class DocumentTypeResponseModel : ContentTypeResponseModelBase<DocumentTypePropertyTypeResponseModel, DocumentTypePropertyTypeContainerResponseModel>
{
    public IEnumerable<ReferenceByIdModel> AllowedTemplates { get; set; } = Enumerable.Empty<ReferenceByIdModel>();

    public ReferenceByIdModel? DefaultTemplate { get; set; }

    public ContentTypeCleanup Cleanup { get; set; } = new();
}
