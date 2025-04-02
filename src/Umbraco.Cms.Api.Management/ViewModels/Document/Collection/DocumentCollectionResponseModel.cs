using Umbraco.Cms.Api.Management.ViewModels.Content;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;

namespace Umbraco.Cms.Api.Management.ViewModels.Document.Collection;

public class DocumentCollectionResponseModel : ContentCollectionResponseModelBase<DocumentValueResponseModel, DocumentVariantResponseModel>
{
    public DocumentTypeCollectionReferenceResponseModel DocumentType { get; set; } = new();

    public bool IsTrashed { get; set; }

    public bool IsProtected { get; set; }

    public Guid[] AncestorIds { get; set; } = [];

    public string? Updater { get; set; }
}
