using System.Linq;

namespace Umbraco.Core.Models.Membership
{
    /// <summary>
    /// Represents an entity permission (defined on the user group and derived to retrieve permissions for a given user)
    /// </summary>
    public class EntityPermission
    {
        public EntityPermission(int groupId, int entityId, string[] assignedPermissions)
        {
            UserGroupId = groupId;
            EntityId = entityId;
            AssignedPermissions = assignedPermissions;
            IsDefaultPermissions = false;
        }

        public EntityPermission(int groupId, int entityId, string[] assignedPermissions, bool isDefaultPermissions)
        {
            UserGroupId = groupId;
            EntityId = entityId;
            AssignedPermissions = assignedPermissions;
            IsDefaultPermissions = isDefaultPermissions;
        }

        public int EntityId { get; private set; }
        public int UserGroupId { get; private set; }

        /// <summary>
        /// The assigned permissions for the user/entity combo
        /// </summary>
        public string[] AssignedPermissions { get; private set; }

        /// <summary>
        /// True if the permissions assigned to this object are the group's default permissions and not explicitly defined permissions
        /// </summary>
        /// <remarks>
        /// This will be the case when looking up entity permissions and falling back to the default permissions
        /// </remarks>
        public bool IsDefaultPermissions { get; private set; }

        /// <summary>
        /// Adds additional permissions to an existing instance of <see cref="EntityPermission"/>
        /// ensuring that only ones that aren't already assigned are added
        /// </summary>
        /// <param name="additionalPermissions"></param>
        public void AddAdditionalPermissions(string[] additionalPermissions)
        {
            //TODO: Fix the performance of this, we need to use things like HashSet and equality checkers, we are iterating too much

            var newPermissions = AssignedPermissions.ToList();
            newPermissions.AddRange(additionalPermissions);
            AssignedPermissions = newPermissions
                .Distinct()
                .ToArray();
        }


    }

}