namespace Umbraco.Cms.Core.Models.Membership;

/// <summary>
///     Represents an entity permission (defined on the user group and derived to retrieve permissions for a given user)
/// </summary>
public class EntityPermission : IEquatable<EntityPermission>
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

    public int EntityId { get; }

    public int UserGroupId { get; }

    /// <summary>
    ///     The assigned permissions for the user/entity combo
    /// </summary>
    public string[] AssignedPermissions { get; }

    /// <summary>
    ///     True if the permissions assigned to this object are the group's default permissions and not explicitly defined
    ///     permissions
    /// </summary>
    /// <remarks>
    ///     This will be the case when looking up entity permissions and falling back to the default permissions
    /// </remarks>
    public bool IsDefaultPermissions { get; }

    public bool Equals(EntityPermission? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return EntityId == other.EntityId && UserGroupId == other.UserGroupId;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((EntityPermission)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (EntityId * 397) ^ UserGroupId;
        }
    }
}
