using Umbraco.Cms.Api.Management.ViewModels.Content;

namespace Umbraco.Cms.Api.Management.ViewModels.Document;

public class DocumentViewModel : ContentViewModelBase<DocumentPropertyViewModel, DocumentVariantViewModel>
{
    public IEnumerable<DocumentUrlInfo> Urls { get; set; } = Array.Empty<DocumentUrlInfo>();

    public Guid? TemplateKey { get; set; }
}
