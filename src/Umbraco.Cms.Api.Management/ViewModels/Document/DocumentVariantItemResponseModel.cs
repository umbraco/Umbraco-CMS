using Umbraco.Cms.Api.Management.ViewModels.Content;

namespace Umbraco.Cms.Api.Management.ViewModels.Document;

public class DocumentVariantItemResponseModel : VariantItemResponseModelBase
{
    public required DocumentVariantState State { get; set; }
}
