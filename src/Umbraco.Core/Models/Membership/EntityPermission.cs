using System.Linq;

namespace Umbraco.Core.Models.Membership
{
    /// <summary>
    /// Represents a user -> entity permission
    /// </summary>
    public class EntityPermission
    {
        public EntityPermission(int userId, int entityId, string[] assignedPermissions)
        {
            UserId = userId;
            EntityId = entityId;
            AssignedPermissions = assignedPermissions;
        }

        public int UserId { get; private set; }
        public int EntityId { get; private set; }

        /// <summary>
        /// The assigned permissions for the user/entity combo
        /// </summary>
        public string[] AssignedPermissions { get; private set; }

        /// <summary>
        /// Adds additional permissions to an existing instance of <see cref="EntityPermission"/>
        /// ensuring that only ones that aren't already assigned are added
        /// </summary>
        /// <param name="additionalPermissions"></param>
        public void AddAdditionalPermissions(string[] additionalPermissions)
        {
            var newPermissions = AssignedPermissions.ToList();
            newPermissions.AddRange(additionalPermissions);
            AssignedPermissions = newPermissions
                .Distinct()
                .ToArray();
        }
    }

}