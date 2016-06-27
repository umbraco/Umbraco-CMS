namespace Umbraco.Core.Models.Membership
{
    /// <summary>
    /// Represents a user group -> entity permission
    /// </summary>
    public class GroupEntityPermission : EntityPermission
    {
        public GroupEntityPermission(int groupId, int entityId, string[] assignedPermissions)
            : base(entityId, assignedPermissions)
        {
            GroupId = groupId;
        }

        public int GroupId { get; private set; }
    }
}