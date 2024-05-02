using Umbraco.Cms.Api.Management.ViewModels.Item;

namespace Umbraco.Cms.Api.Management.ViewModels.User.Item;

public class UserItemResponseModel : NamedItemResponseModelBase
{
    public IEnumerable<string> AvatarUrls { get; set; } = Enumerable.Empty<string>();
}
