using Umbraco.Cms.Api.Management.ViewModels.Content;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;

namespace Umbraco.Cms.Api.Management.ViewModels.Document;

public class DocumentVersionResponseModel : ContentResponseModelBase<DocumentValueModel, DocumentVariantResponseModel>
{
    public ReferenceByIdModel? Document { get; set; }

    public DocumentTypeReferenceResponseModel DocumentType { get; set; } = new();
}
