using Umbraco.Cms.Api.Management.ViewModels.Item;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.ViewModels.MemberGroup.Item;

public class MemberGroupItemResponseModel : ItemResponseModelBase
{
    public override string Type => Constants.UdiEntityType.MemberGroup;
}
