using Umbraco.Cms.Api.Common.Attributes;
using Umbraco.Cms.Api.Management.ViewModels.Content;

namespace Umbraco.Cms.Api.Management.ViewModels.Document;

[ShortGenericSchemaName<DocumentValueModel, DocumentVariantResponseModel>("ContentForDocumentResponseModel")]
public class DocumentResponseModel : ContentResponseModelBase<DocumentValueModel, DocumentVariantResponseModel>
{
    public IEnumerable<ContentUrlInfo> Urls { get; set; } = Enumerable.Empty<ContentUrlInfo>();

    public Guid? TemplateId { get; set; }

    public bool IsTrashed { get; set; }
}
