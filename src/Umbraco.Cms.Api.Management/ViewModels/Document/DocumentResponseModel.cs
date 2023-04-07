using Umbraco.Cms.Api.Management.ViewModels.Content;

namespace Umbraco.Cms.Api.Management.ViewModels.Document;

public class DocumentResponseModel : ContentResponseModelBase<DocumentValueModel, DocumentVariantResponseModel>
{
    public IEnumerable<ContentUrlInfo> Urls { get; set; } = Array.Empty<ContentUrlInfo>();

    public Guid? TemplateId { get; set; }
}
