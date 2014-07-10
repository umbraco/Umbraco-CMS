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
        /// The key/value pairs of user id & single permission
        /// </summary>
        public IEnumerable<UserPermission> UserPermissionsSet { get; private set; }

        public EntityPermissionSet(int entityId, IEnumerable<UserPermission> userPermissionsSet)
        {
            EntityId = entityId;
            UserPermissionsSet = userPermissionsSet;
        }

        public class UserPermission
        {
            public UserPermission(int userId, string permission)
            {
                UserId = userId;
                Permission = permission;
            }

            public int UserId { get; private set; }
            public string Permission { get; private set; }

            protected bool Equals(UserPermission other)
            {
                return UserId == other.UserId && string.Equals(Permission, other.Permission);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((UserPermission) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (UserId*397) ^ Permission.GetHashCode();
                }
            }
        }
    }
}