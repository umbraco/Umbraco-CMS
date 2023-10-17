using Umbraco.Cms.Api.Management.ViewModels.Item;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.ViewModels.DocumentType.Item;

public class DocumentTypeItemResponseModel : ItemResponseModelBase
{
    public bool IsElement { get; set; }

    public string? Icon { get; set; }

    public override string Type => Constants.UdiEntityType.DocumentType;
}
