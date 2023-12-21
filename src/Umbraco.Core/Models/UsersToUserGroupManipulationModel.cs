using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Core.Models;

public class UsersToUserGroupManipulationModel
{
    public Guid UserGroupKey { get; init; }
    public Guid[] UserKeys { get; init; }

    public UsersToUserGroupManipulationModel(Guid userGroup, Guid[] userKeys)
    {
        UserGroupKey = userGroup;
        UserKeys = userKeys;
    }
}
