namespace Umbraco.Cms.Api.Management.ViewModels.Document;

public class DocumentResponseModel : DocumentResponseModelBase<DocumentValueResponseModel, DocumentVariantResponseModel>
{
    [Obsolete("This property is no longer populated. Please use /document/{id}/urls instead to retrieve the URLs for a document. Scheduled for removal in Umbraco 17.")]
    public IEnumerable<DocumentUrlInfo> Urls { get; set; } = Enumerable.Empty<DocumentUrlInfo>();

    public ReferenceByIdModel? Template { get; set; }

    public bool IsTrashed { get; set; }
}
