namespace Umbraco.Cms.Api.Management.ViewModels.Users;

public class UpdateUserGroupsOnUserRequestModel
{
    public required SortedSet<Guid> UserIds { get; set; }

    public required SortedSet<Guid> UserGroupIds { get; set; }
}
