using Umbraco.Cms.Api.Management.ViewModels.Content;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;

namespace Umbraco.Cms.Api.Management.ViewModels.Document.Collection;

public class DocumentCollectionResponseModel : ContentResponseModelBase<DocumentValueModel, DocumentVariantResponseModel>
{
    public int SortOrder { get; set; }

    public DocumentTypeCollectionReferenceResponseModel DocumentType { get; set; } = new();
}
