using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Core.Models;

public class UserGroupUpdateModel
{
    /// <summary>
    /// the usergroup to update
    /// </summary>
    public IUserGroup UserGroup { get; init; }

    /// <summary>
    /// the list of users to explicitly assign to the group
    /// </summary>
    public Guid[]? GroupUserKeys { get; init; }

    public UserGroupUpdateModel(IUserGroup userGroup, Guid[]? groupUserKeys)
    {
        UserGroup = userGroup;
        GroupUserKeys = groupUserKeys;
    }
}
