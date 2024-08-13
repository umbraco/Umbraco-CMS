using Umbraco.Cms.Api.Management.ViewModels.Item;

namespace Umbraco.Cms.Api.Management.ViewModels.UserGroup.Item;

public class UserGroupItemResponseModel : NamedItemResponseModelBase
{
    public string? Icon { get; set; }

    public string? Alias { get; set; }
}
