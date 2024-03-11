namespace Umbraco.Cms.Core.Models;

public class UsersToUserGroupManipulationModel
{
    public Guid UserGroupKey { get; init; }

    public Guid[] UserKeys { get; init; }

    public UsersToUserGroupManipulationModel(Guid userGroupKey, Guid[] userKeys)
    {
        UserGroupKey = userGroupKey;
        UserKeys = userKeys;
    }
}
