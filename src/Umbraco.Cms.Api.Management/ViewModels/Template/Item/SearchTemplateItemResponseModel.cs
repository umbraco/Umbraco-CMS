using Umbraco.Cms.Api.Management.ViewModels.Item;

namespace Umbraco.Cms.Api.Management.ViewModels.Template.Item;

public class SearchTemplateItemResponseModel : TemplateItemResponseModel
{
    public IEnumerable<SearchResultAncestorModel> Ancestors { get; set; } = [];
}
