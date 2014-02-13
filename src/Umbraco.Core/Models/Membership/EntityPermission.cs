﻿namespace Umbraco.Core.Models.Membership
{
    /// <summary>
    /// Represents a user -> entity permission
    /// </summary>
    public class EntityPermission
    {
        public EntityPermission(object userId, int entityId, string[] assignedPermissions)
        {
            UserId = userId;
            EntityId = entityId;
            AssignedPermissions = assignedPermissions;
        }

        public object UserId { get; private set; }
        public int EntityId { get; private set; }

        /// <summary>
        /// The assigned permissions for the user/entity combo
        /// </summary>
        public string[] AssignedPermissions { get; private set; }
    }

}