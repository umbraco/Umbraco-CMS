using System.Linq;

namespace Umbraco.Core.Models.Membership
{
    /// <summary>
    /// Represents an entity permission (defined on the user group and derived to retrieve permissions for a given user)
    /// </summary>
    public class EntityPermission
    {
        public EntityPermission(int entityId, string[] assignedPermissions)
        {
            EntityId = entityId;
            AssignedPermissions = assignedPermissions;
        }

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