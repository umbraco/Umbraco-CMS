namespace Umbraco.Cms.Core.Models.Membership;

/// <summary>
///     Represents an entity permission (defined on the user group and derived to retrieve permissions for a given user)
/// </summary>
public class EntityPermission : IEquatable<EntityPermission>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="EntityPermission" /> class.
    /// </summary>
    /// <param name="groupId">The user group identifier.</param>
    /// <param name="entityId">The entity identifier.</param>
    /// <param name="assignedPermissions">The set of assigned permissions.</param>
    public EntityPermission(int groupId, int entityId, ISet<string> assignedPermissions)
    {
        UserGroupId = groupId;
        EntityId = entityId;
        AssignedPermissions = assignedPermissions;
        IsDefaultPermissions = false;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="EntityPermission" /> class.
    /// </summary>
    /// <param name="groupId">The user group identifier.</param>
    /// <param name="entityId">The entity identifier.</param>
    /// <param name="assignedPermissions">The set of assigned permissions.</param>
    /// <param name="isDefaultPermissions">Indicates whether these are default permissions.</param>
    public EntityPermission(int groupId, int entityId, ISet<string> assignedPermissions, bool isDefaultPermissions)
    {
        UserGroupId = groupId;
        EntityId = entityId;
        AssignedPermissions = assignedPermissions;
        IsDefaultPermissions = isDefaultPermissions;
    }

    /// <summary>
    ///     Gets the entity identifier.
    /// </summary>
    public int EntityId { get; }

    /// <summary>
    ///     Gets the user group identifier.
    /// </summary>
    public int UserGroupId { get; }

    /// <summary>
    ///     The assigned permissions for the user/entity combo
    /// </summary>
    public ISet<string> AssignedPermissions { get; }

    /// <summary>
    ///     True if the permissions assigned to this object are the group's default permissions and not explicitly defined
    ///     permissions
    /// </summary>
    /// <remarks>
    ///     This will be the case when looking up entity permissions and falling back to the default permissions
    /// </remarks>
    public bool IsDefaultPermissions { get; }

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
    public override int GetHashCode()
    {
        unchecked
        {
            return (EntityId * 397) ^ UserGroupId;
        }
    }
}
