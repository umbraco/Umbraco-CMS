using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Core.Models.Membership
{
    /// <summary>
    /// Represents an entity -> user group & permission key value pair collection
    /// </summary>    
    public class EntityPermissionSet
    {
        /// <summary>
        /// Returns an empty permission set
        /// </summary>
        /// <returns></returns>
        public static EntityPermissionSet Empty()
        {
            return new EntityPermissionSet(-1, new EntityPermission[0]);
        }

        public EntityPermissionSet(int entityId, IEnumerable<EntityPermission> permissionsSet)
        {
            EntityId = entityId;
            PermissionsSet = permissionsSet;
        }

        /// <summary>
        /// The entity id with permissions assigned
        /// </summary>
        public virtual int EntityId { get; private set; }

        /// <summary>
        /// The key/value pairs of user group id & single permission
        /// </summary>
        public IEnumerable<EntityPermission> PermissionsSet { get; private set; }

        /// <summary>
        /// Returns the aggregte permissions in the permission set
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// This value is only calculated once
        /// </remarks>
        public IEnumerable<string> GetAllPermissions()
        {
            return _calculatedPermissions ?? (_calculatedPermissions =
                       PermissionsSet.SelectMany(x => x.AssignedPermissions).Distinct().ToArray());
        }

        private string[] _calculatedPermissions;




       
    }
}