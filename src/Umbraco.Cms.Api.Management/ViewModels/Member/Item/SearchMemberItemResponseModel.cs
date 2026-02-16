using Umbraco.Cms.Api.Management.ViewModels.Item;

namespace Umbraco.Cms.Api.Management.ViewModels.Member.Item;

public class SearchMemberItemResponseModel : MemberItemResponseModel
{
    public IEnumerable<SearchResultAncestorModel> Ancestors { get; set; } = [];
}
