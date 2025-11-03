using Umbraco.Cms.Api.Management.ViewModels.Content;
using Umbraco.Cms.Api.Management.ViewModels.Document;

namespace Umbraco.Cms.Api.Management.ViewModels.Element;

public class ElementVariantItemResponseModel : VariantItemResponseModelBase
{
    public required DocumentVariantState State { get; set; }
}
