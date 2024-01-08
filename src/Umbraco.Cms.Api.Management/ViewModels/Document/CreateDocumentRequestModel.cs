using Umbraco.Cms.Api.Common.Attributes;
using Umbraco.Cms.Api.Management.ViewModels.Content;

namespace Umbraco.Cms.Api.Management.ViewModels.Document;

[ShortGenericSchemaName<DocumentValueModel, DocumentVariantRequestModel>("CreateContentForDocumentRequestModel")]
public class CreateDocumentRequestModel : CreateContentRequestModelBase<DocumentValueModel, DocumentVariantRequestModel>
{
    public required ReferenceByIdRequestModel DocumentType { get; set; }

    public required ReferenceByIdRequestModel? Template { get; set; }
}
