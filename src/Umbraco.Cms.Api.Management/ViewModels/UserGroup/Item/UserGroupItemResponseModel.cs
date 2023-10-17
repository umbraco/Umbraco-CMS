using Umbraco.Cms.Api.Management.ViewModels.Item;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.ViewModels.UserGroup.Item;

public class UserGroupItemResponseModel : ItemResponseModelBase
{
    public string? Icon { get; set; }

    public override string Type => Constants.UdiEntityType.UserGroup;
}
