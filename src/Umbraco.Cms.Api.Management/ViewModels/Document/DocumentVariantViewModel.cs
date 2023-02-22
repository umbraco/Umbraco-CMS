using Umbraco.Cms.Api.Management.ViewModels.Content;

namespace Umbraco.Cms.Api.Management.ViewModels.Document;

public class DocumentVariantViewModel : VariantViewModelBase
{
    public ContentState State { get; set; }

    public DateTime? PublishDate { get; set; }
}
