using Umbraco.Cms.Api.Management.ViewModels.Item;

namespace Umbraco.Cms.Api.Management.ViewModels.DocumentType.Item;

public class DocumentTypeItemResponseModel : NamedItemResponseModelBase
{
    public bool IsElement { get; set; }

    public string? Icon { get; set; }

    public string? Description { get; set; }
}
