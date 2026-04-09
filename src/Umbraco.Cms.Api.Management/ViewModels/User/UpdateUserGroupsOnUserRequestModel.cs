namespace Umbraco.Cms.Api.Management.ViewModels.User;

/// <summary>
/// Represents a request model to update the user groups associated with a user.
/// </summary>
public class UpdateUserGroupsOnUserRequestModel
{
    /// <summary>
    /// Gets or sets the collection of user identifiers for which the user groups will be updated.
    /// </summary>
    public required ISet<ReferenceByIdModel> UserIds { get; set; }

    /// <summary>
    /// Gets or sets the set of user group IDs associated with the user.
    /// </summary>
    public required ISet<ReferenceByIdModel> UserGroupIds { get; set; }
}
