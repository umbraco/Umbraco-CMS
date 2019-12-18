using System;
using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Core.Models.Membership
{
    /// <summary>
    /// Represents an entity -> user group & permission key value pair collection
    /// </summary>
    public class EntityPermissionSet
    {
        private static readonly Lazy<EntityPermissionSet> EmptyInstance = new Lazy<EntityPermissionSet>(() => new EntityPermissionSet(-1, new EntityPermissionCollection()));
        /// <summary>
        /// Returns an empty permission set
        /// </summary>
        /// <returns></returns>
        public static EntityPermissionSet Empty()
        {
            return EmptyInstance.Value;
        }

        public EntityPermissionSet(int entityId, EntityPermissionCollection permissionsSet)
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
        public EntityPermissionCollection PermissionsSet { get; private set; }


        /// <summary>
        /// Returns the aggregate permissions in the permission set
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// This value is only calculated once
        /// </remarks>
        public IEnumerable<string> GetAllPermissions()
        {
            return PermissionsSet.GetAllPermissions();
        }




    }
}
