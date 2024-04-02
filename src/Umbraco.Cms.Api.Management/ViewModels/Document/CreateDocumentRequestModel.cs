namespace Umbraco.Cms.Api.Management.ViewModels.Document;

public class CreateDocumentRequestModel : CreateDocumentRequestModelBase<DocumentValueModel, DocumentVariantRequestModel>
{
    public required ReferenceByIdModel? Template { get; set; }
}
