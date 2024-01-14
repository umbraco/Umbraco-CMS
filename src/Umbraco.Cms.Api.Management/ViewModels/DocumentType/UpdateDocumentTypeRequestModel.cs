using Umbraco.Cms.Api.Common.Attributes;
using Umbraco.Cms.Api.Management.ViewModels.ContentType;

namespace Umbraco.Cms.Api.Management.ViewModels.DocumentType;

[ShortGenericSchemaName<UpdateDocumentTypePropertyTypeRequestModel, UpdateDocumentTypePropertyTypeContainerRequestModel>("UpdateContentTypeForDocumentTypeRequestModel")]
public class UpdateDocumentTypeRequestModel
    : UpdateContentTypeRequestModelBase<UpdateDocumentTypePropertyTypeRequestModel, UpdateDocumentTypePropertyTypeContainerRequestModel>
{
    public IEnumerable<ReferenceByIdModel> AllowedTemplates { get; set; } = Enumerable.Empty<ReferenceByIdModel>();

    public ReferenceByIdModel? DefaultTemplate { get; set; }

    public ContentTypeCleanup Cleanup { get; set; } = new();
}
