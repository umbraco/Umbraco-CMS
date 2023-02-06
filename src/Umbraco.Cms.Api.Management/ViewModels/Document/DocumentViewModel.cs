using Umbraco.Cms.Api.Management.ViewModels.Content;

namespace Umbraco.Cms.Api.Management.ViewModels.Document;

public class DocumentViewModel : ContentViewModelBase<DocumentPropertyViewModel, DocumentVariantViewModel>
{
    public IEnumerable<ContentUrlInfo> Urls { get; set; } = Array.Empty<ContentUrlInfo>();

    public Guid? TemplateKey { get; set; }
}
