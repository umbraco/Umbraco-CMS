using Umbraco.Cms.Core;
using Umbraco.Cms.Api.Management.ViewModels.Item;

namespace Umbraco.Cms.Api.Management.ViewModels.UserGroup.Item;

public class UserGroupItemResponseModel : ItemResponseModelBase
{
    public string? Icon { get; set; }
    public override string Type => UdiEntityType.UserGroup;
}
