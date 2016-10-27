using System.Collections.Generic;

namespace Umbraco.Core.Models.Membership
{
    /// <summary>
    /// Represents an entity -> user & permission key value pair collection
    /// </summary>
    public class EntityPermissionSet
    {
        /// <summary>
        /// The entity id with permissions assigned
        /// </summary>
        public int EntityId { get; private set; }

        /// <summary>
        /// The key/value pairs of user group id & single permission
        /// </summary>
        public IEnumerable<UserGroupPermission> PermissionsSet { get; private set; }

        public EntityPermissionSet(int entityId, IEnumerable<UserGroupPermission> permissionsSet)
        {
            EntityId = entityId;
            PermissionsSet = permissionsSet;
        }

        public class UserGroupPermission
        {
            public UserGroupPermission(int groupId, string permission)
            {
                UserGroupId = groupId;
                Permission = permission;
            }

            public int UserGroupId { get; private set; }

            public string Permission { get; private set; }

            protected bool Equals(UserGroupPermission other)
            {
                return UserGroupId == other.UserGroupId && string.Equals(Permission, other.Permission);
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
                    return (UserGroupId * 397) ^ Permission.GetHashCode();
                }
            }
        }
    }
}