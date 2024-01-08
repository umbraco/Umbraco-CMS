using Umbraco.Cms.Api.Common.Attributes;
using Umbraco.Cms.Api.Management.ViewModels.Content;

namespace Umbraco.Cms.Api.Management.ViewModels.Document;

[ShortGenericSchemaName<DocumentValueModel, DocumentVariantRequestModel>("UpdateContentForDocumentRequestModel")]
public class UpdateDocumentRequestModel : UpdateContentRequestModelBase<DocumentValueModel, DocumentVariantRequestModel>
{
    public ReferenceByIdRequestModel? Template { get; set; }
}
