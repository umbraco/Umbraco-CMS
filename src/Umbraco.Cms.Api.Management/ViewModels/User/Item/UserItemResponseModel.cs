using Umbraco.Cms.Api.Management.ViewModels.Item;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.ViewModels.User.Item;

public class UserItemResponseModel : ItemResponseModelBase
{
    public override string Type => Constants.UdiEntityType.User;
}
