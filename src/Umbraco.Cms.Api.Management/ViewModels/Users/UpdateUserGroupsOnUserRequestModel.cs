namespace Umbraco.Cms.Api.Management.ViewModels.Users;

public class UpdateUserGroupsOnUserRequestModel
{
    public required SortedSet<Guid> UserKeys { get; set; }

    public required SortedSet<Guid> UserGroupKeys { get; set; }
}
