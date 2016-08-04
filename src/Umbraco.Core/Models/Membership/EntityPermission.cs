using System;
using System.Linq;

namespace Umbraco.Core.Models.Membership
{
    [Serializable]
    public abstract class EntityPermission
    {
        protected EntityPermission(int entityId, string[] assignedPermissions)
        {
            EntityId = entityId;
            AssignedPermissions = assignedPermissions;
        }

        public int EntityId { get; private set; }

        /// <summary>
        /// The assigned permissions for the user or group/entity combo
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