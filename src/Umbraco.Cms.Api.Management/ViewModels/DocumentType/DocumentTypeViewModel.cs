using Umbraco.Cms.Api.Management.ViewModels.ContentType;

namespace Umbraco.Cms.Api.Management.ViewModels.DocumentType;

public class DocumentTypeViewModel : ContentTypeViewModelBase<DocumentTypePropertyTypeViewModel, DocumentTypePropertyTypeContainerViewModel>
{
    public IEnumerable<Guid> AllowedTemplateKeys { get; set; } = Array.Empty<Guid>();

    public Guid? DefaultTemplateKey { get; set; }

    // FIXME: either move this to base level or rename to DocumentTypeCleanup
    public ContentTypeCleanup Cleanup { get; set; } = new();
}
