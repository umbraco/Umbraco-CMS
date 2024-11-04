namespace Umbraco.Cms.Api.Management.ViewModels.Document;

public class DocumentVersionResponseModel : DocumentResponseModelBase<DocumentValueResponseModel, DocumentVariantResponseModel>
{
    public ReferenceByIdModel? Document { get; set; }
}
