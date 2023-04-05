using Umbraco.Cms.Api.Management.ViewModels.Item;

namespace Umbraco.Cms.Api.Management.ViewModels.DocumentType.Item;

public class DocumentTypeItemResponseModel : ItemResponseModelBase
{
    public bool IsElement { get; set; }

    public string? Icon { get; set; }
}
