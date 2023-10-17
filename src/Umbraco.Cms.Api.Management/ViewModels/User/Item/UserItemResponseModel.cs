using Umbraco.Cms.Api.Management.ViewModels.Item;

namespace Umbraco.Cms.Api.Management.ViewModels.User.Item;

public class UserItemResponseModel : ItemResponseModelBase
{
    public override string Type => Constants.ResourceObjectTypes.User;
}
