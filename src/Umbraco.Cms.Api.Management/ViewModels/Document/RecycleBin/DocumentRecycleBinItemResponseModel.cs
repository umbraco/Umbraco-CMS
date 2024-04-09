using Umbraco.Cms.Api.Management.ViewModels.Content;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Api.Management.ViewModels.RecycleBin;

namespace Umbraco.Cms.Api.Management.ViewModels.Document.RecycleBin;

public class DocumentRecycleBinItemResponseModel : RecycleBinItemResponseModelBase
{
    public DocumentTypeReferenceResponseModel DocumentType { get; set; } = new();

    public IEnumerable<DocumentVariantItemResponseModel> Variants { get; set; } = Enumerable.Empty<DocumentVariantItemResponseModel>();
}
