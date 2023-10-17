using Umbraco.Cms.Api.Management.ViewModels.Item;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.ViewModels.Member.Item;

public class MemberItemResponseModel : ItemResponseModelBase
{
    public string? Icon { get; set; }

    public override string Type => Constants.UdiEntityType.Member;
}
