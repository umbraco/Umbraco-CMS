using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Api.Management.ViewModels.RecycleBin;

namespace Umbraco.Cms.Api.Management.ViewModels.Element.RecycleBin;

public class ElementRecycleBinItemResponseModel : RecycleBinItemResponseModelBase
{
    public DocumentTypeReferenceResponseModel? DocumentType { get; set; } = new();

    public IEnumerable<ElementVariantItemResponseModel> Variants { get; set; } = Enumerable.Empty<ElementVariantItemResponseModel>();

    public bool IsFolder { get; set; }

    public string Name { get; set; } = string.Empty;
}
