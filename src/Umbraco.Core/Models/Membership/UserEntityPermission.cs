using System;

namespace Umbraco.Core.Models.Membership
{
    /// <summary>
    /// Represents a user -> entity permission
    /// </summary>
    /// <remarks>Made seriazable to support cloning via <see cref="ObjectExtensions"/> DeepClone method</remarks>
    [Serializable]
    public class UserEntityPermission : EntityPermission
    {
        public UserEntityPermission(int userId, int entityId, string[] assignedPermissions)
            : base(entityId, assignedPermissions)
        {
            UserId = userId;
        }

        public int UserId { get; private set; }
    }
}