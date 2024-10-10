namespace Umbraco.Cms.Api.Management.ViewModels.UserGroup;

public class DeleteUserGroupsRequestModel
{
    public HashSet<ReferenceByIdModel> UserGroupIds { get; set; } = new();
}
