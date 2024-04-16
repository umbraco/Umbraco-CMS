namespace Umbraco.Cms.Api.Management.ViewModels.Document;

public class UpdateDocumentRequestModel : UpdateDocumentRequestModelBase<DocumentValueModel, DocumentVariantRequestModel>
{
    public ReferenceByIdModel? Template { get; set; }
}
