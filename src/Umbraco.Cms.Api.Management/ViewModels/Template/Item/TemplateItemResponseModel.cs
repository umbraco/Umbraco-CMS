using Umbraco.Cms.Api.Management.ViewModels.Item;

namespace Umbraco.Cms.Api.Management.ViewModels.Template.Item;

public class TemplateItemResponseModel : ItemResponseModelBase
{
    public required string Alias { get; set; }
}
