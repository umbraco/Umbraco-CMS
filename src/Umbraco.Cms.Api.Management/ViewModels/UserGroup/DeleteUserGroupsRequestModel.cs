namespace Umbraco.Cms.Api.Management.ViewModels.UserGroup;

/// <summary>
/// Represents the data required to request the deletion of one or more user groups.
/// </summary>
public class DeleteUserGroupsRequestModel
{
    /// <summary>
    /// Gets or sets the collection of user group IDs to be deleted.
    /// </summary>
    public HashSet<ReferenceByIdModel> UserGroupIds { get; set; } = new();
}
