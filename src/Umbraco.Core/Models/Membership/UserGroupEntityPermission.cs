namespace Umbraco.Core.Models.Membership
{
    /// <summary>
    /// Represents a user group -> entity permission
    /// </summary>
    public class UserGroupEntityPermission : EntityPermission
    {
        public UserGroupEntityPermission(int groupId, int entityId, string[] assignedPermissions)
            : base(entityId, assignedPermissions)
        {
            UserGroupId = groupId;
        }

        public int UserGroupId { get; private set; }
    }
}