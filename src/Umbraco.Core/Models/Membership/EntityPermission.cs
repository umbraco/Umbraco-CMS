namespace Umbraco.Core.Models.Membership
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

        public object UserId { get; set; }
        public int EntityId { get; set; }

        /// <summary>
        /// The assigned permissions for the user/entity combo
        /// </summary>
        public string[] AssignedPermissions { get; set; }
    }
}