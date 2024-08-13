using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Api.Management.ViewModels.Item;

namespace Umbraco.Cms.Api.Management.ViewModels.DocumentBlueprint.Item;

public class DocumentBlueprintItemResponseModel : NamedItemResponseModelBase
{
    public DocumentTypeReferenceResponseModel DocumentType { get; set; } = new();
}
