using System;
using Umbraco.Core.Models.Membership;

namespace Umbraco.Core.Events
{
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
}
