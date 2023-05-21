using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Core.Events;

public class UserGroupWithUsers
{
    public UserGroupWithUsers(IUserGroup userGroup, IUser[] addedUsers, IUser[] removedUsers)
    {
        UserGroup = userGroup;
        AddedUsers = addedUsers;
        RemovedUsers = removedUsers;
    }

    public IUserGroup UserGroup { get; }

    public IUser[] AddedUsers { get; }

    public IUser[] RemovedUsers { get; }
}
