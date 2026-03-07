namespace Umbraco.Cms.Api.Management.ViewModels.UserGroup;

/// <summary>
/// Represents a data transfer object containing information about a user group returned by the API.
/// </summary>
public class UserGroupResponseModel : UserGroupBase
{
    /// <summary>
    /// The key identifier for the user group.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Whether this user group is required at system level (thus cannot be removed)
    /// </summary>
    public bool IsDeletable { get; set; }

    /// <summary>
    /// Whether this user group is required at system level (thus alias needs to be fixed)
    /// </summary>
    public bool AliasCanBeChanged { get; set; }
}
