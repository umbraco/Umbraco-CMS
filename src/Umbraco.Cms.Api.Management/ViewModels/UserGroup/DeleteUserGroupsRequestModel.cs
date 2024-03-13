namespace Umbraco.Cms.Api.Management.ViewModels.UserGroup;

public class DeleteUserGroupsRequestModel
{
    public HashSet<Guid> UserGroupIds { get; set; } = new();
}
