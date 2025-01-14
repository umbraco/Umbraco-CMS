using Umbraco.Cms.Api.Management.ViewModels.Item;
using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Api.Management.ViewModels.User.Item;

public class UserItemResponseModel : NamedItemResponseModelBase
{
    public IEnumerable<string> AvatarUrls { get; set; } = Enumerable.Empty<string>();

    public UserKind Kind { get; set; }
}
