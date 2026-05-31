using Umbraco.Cms.Api.Management.ViewModels.Item;

namespace Umbraco.Cms.Api.Management.ViewModels.UserGroup.Item;

/// <summary>
/// Represents the data returned for a user group item in API responses.
/// Typically includes information such as the group's identifier, name, and permissions.
/// </summary>
public class UserGroupItemResponseModel : NamedItemResponseModelBase
{
    /// <summary>Gets or sets the icon associated with the user group.</summary>
    public string? Icon { get; set; }

    /// <summary>
    /// Gets or sets the alias of the user group.
    /// </summary>
    public string? Alias { get; set; }
}
