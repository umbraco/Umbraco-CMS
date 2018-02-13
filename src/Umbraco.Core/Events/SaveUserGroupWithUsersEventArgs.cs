using System;
using Umbraco.Core.Models.Membership;

namespace Umbraco.Core.Events
{
    internal class SaveUserGroupWithUsersEventArgs : EventArgs
    {
        public SaveUserGroupWithUsersEventArgs(IUserGroup userGroup, IUser[] addedUsers, IUser[] removedUsers)
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
