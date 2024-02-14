using Umbraco.Cms.Api.Management.ViewModels.Content;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;

namespace Umbraco.Cms.Api.Management.ViewModels.Document.Collection;

public class DocumentCollectionResponseModel : ContentCollectionResponseModelBase<DocumentValueModel, DocumentVariantResponseModel>
{
    public string? Updater { get; set; }

    public DocumentTypeCollectionReferenceResponseModel DocumentType { get; set; } = new();
}
