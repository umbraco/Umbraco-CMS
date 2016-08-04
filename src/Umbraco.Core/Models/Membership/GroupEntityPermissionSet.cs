using System.Collections.Generic;

namespace Umbraco.Core.Models.Membership
{
    /// <summary>
    /// Represents an entity -> group & permission key value pair collection
    /// </summary>
    public class GroupEntityPermissionSet : EntityPermissionSet
    {
        /// <summary>
        /// The key/value pairs of group id & single permission
        /// </summary>
        public IEnumerable<UserGroupPermission> PermissionsSet { get; private set; }

        public GroupEntityPermissionSet(int entityId, IEnumerable<UserGroupPermission> permissionsSet)
            : base (entityId)
        {
            PermissionsSet = permissionsSet;
        }

        public class UserGroupPermission
        {
            public UserGroupPermission(int groupId, string permission)
            {
                GroupId = groupId;
                Permission = permission;
            }

            public int GroupId { get; private set; }

            public string Permission { get; private set; }

            protected bool Equals(UserGroupPermission other)
            {
                return GroupId == other.GroupId && string.Equals(Permission, other.Permission);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((UserGroupPermission) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (GroupId*397) ^ Permission.GetHashCode();
                }
            }
        }
    }
}