using Umbraco.Cms.Api.Management.ViewModels.Content;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.ViewModels.Document;

public class DocumentResponseModel : ContentResponseModelBase<DocumentValueModel, DocumentVariantResponseModel>
{
    public IEnumerable<ContentUrlInfo> Urls { get; set; } = Array.Empty<ContentUrlInfo>();

    public Guid? TemplateId { get; set; }

    public string Type => Constants.UdiEntityType.Document;
}
