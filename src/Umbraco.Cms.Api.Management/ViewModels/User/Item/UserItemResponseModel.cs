using Umbraco.Cms.Api.Management.ViewModels.Item;
using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Api.Management.ViewModels.User.Item;

/// <summary>
/// Represents a response model containing details about a user item, typically returned by the management API.
/// </summary>
public class UserItemResponseModel : NamedItemResponseModelBase
{
    /// <summary>
    /// Gets or sets the collection of avatar URLs associated with the user.
    /// </summary>
    public IEnumerable<string> AvatarUrls { get; set; } = Enumerable.Empty<string>();

    /// <summary>
    /// Gets or sets the type of user, indicating their role or classification within the system.
    /// </summary>
    public UserKind Kind { get; set; }
}
