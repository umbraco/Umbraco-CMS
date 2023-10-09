using Umbraco.Cms.Api.Management.ViewModels.Abstract;
using Umbraco.Cms.Api.Management.ViewModels.Content;

namespace Umbraco.Cms.Api.Management.ViewModels.Document;

public class DocumentResponseModel : ContentResponseModelBase<DocumentValueModel, DocumentVariantResponseModel>,
    ITracksTrashing
{
    public IEnumerable<ContentUrlInfo> Urls { get; set; } = Array.Empty<ContentUrlInfo>();

    public Guid? TemplateId { get; set; }
    public bool IsTrashed { get; set; }
}
