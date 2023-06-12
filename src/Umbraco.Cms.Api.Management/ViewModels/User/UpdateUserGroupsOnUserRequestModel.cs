namespace Umbraco.Cms.Api.Management.ViewModels.User;

public class UpdateUserGroupsOnUserRequestModel
{
    public required ISet<Guid> UserIds { get; set; }

    public required ISet<Guid> UserGroupIds { get; set; }
}
