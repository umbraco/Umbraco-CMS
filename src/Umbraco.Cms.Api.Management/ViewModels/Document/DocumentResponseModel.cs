namespace Umbraco.Cms.Api.Management.ViewModels.Document;

public class DocumentResponseModel : DocumentResponseModelBase<DocumentValueResponseModel, DocumentVariantResponseModel>
{
    public ReferenceByIdModel? Template { get; set; }

    public bool IsTrashed { get; set; }
}
