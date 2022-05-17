namespace Umbraco.Cms.Core.Models.Membership;

/// <summary>
///     Represents an entity -&gt; user group & permission key value pair collection
/// </summary>
public class EntityPermissionSet
{
    private static readonly Lazy<EntityPermissionSet> EmptyInstance =
        new(() => new EntityPermissionSet(-1, new EntityPermissionCollection()));

    public EntityPermissionSet(int entityId, EntityPermissionCollection permissionsSet)
    {
        EntityId = entityId;
        PermissionsSet = permissionsSet;
    }

    /// <summary>
    ///     The entity id with permissions assigned
    /// </summary>
    public virtual int EntityId { get; }

    /// <summary>
    ///     The key/value pairs of user group id & single permission
    /// </summary>
    public EntityPermissionCollection PermissionsSet { get; }

    /// <summary>
    ///     Returns an empty permission set
    /// </summary>
    /// <returns></returns>
    public static EntityPermissionSet Empty() => EmptyInstance.Value;

    /// <summary>
    ///     Returns the aggregate permissions in the permission set
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    ///     This value is only calculated once
    /// </remarks>
    public IEnumerable<string> GetAllPermissions() => PermissionsSet.GetAllPermissions();
}
