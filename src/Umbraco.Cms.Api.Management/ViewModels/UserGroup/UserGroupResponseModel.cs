namespace Umbraco.Cms.Api.Management.ViewModels.UserGroup;

public class UserGroupResponseModel : UserGroupBase
{
    /// <summary>
    /// The key identifier for the user group.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Whether this user group is required at system level (thus cannot be removed)
    /// </summary>
    public bool IsSystemGroup { get; set; }
}
