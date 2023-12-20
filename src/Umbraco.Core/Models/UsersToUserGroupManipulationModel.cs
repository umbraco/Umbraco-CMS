using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Core.Models;

public class UsersToUserGroupManipulationModel
{
    public IUserGroup UserGroup { get; init; }
    public Guid[] UserKeys { get; init; }

    public UsersToUserGroupManipulationModel(IUserGroup userGroup, Guid[] userKeys)
    {
        UserGroup = userGroup;
        UserKeys = userKeys;
    }
}
