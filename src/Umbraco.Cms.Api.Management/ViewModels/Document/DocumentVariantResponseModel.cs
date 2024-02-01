using Umbraco.Cms.Api.Management.ViewModels.Content;

namespace Umbraco.Cms.Api.Management.ViewModels.Document;

public class DocumentVariantResponseModel : VariantResponseModelBase
{
    public ContentState State { get; set; }

    public DateTimeOffset? PublishDate { get; set; }
}
