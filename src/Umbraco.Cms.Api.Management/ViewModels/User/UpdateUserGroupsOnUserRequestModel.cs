namespace Umbraco.Cms.Api.Management.ViewModels.User;

public class UpdateUserGroupsOnUserRequestModel
{
    public required ISet<ReferenceByIdModel> UserIds { get; set; }

    public required ISet<ReferenceByIdModel> UserGroupIds { get; set; }
}
