namespace Umbraco.Cms.Api.Management.ViewModels.Document;

public class DocumentVersionResponseModel : DocumentResponseModelBase<DocumentValueModel, DocumentVariantResponseModel>
{
    public ReferenceByIdModel? Document { get; set; }
}
