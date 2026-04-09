namespace Umbraco.Cms.Core.Models.Membership;

/// <summary>
///     Represents an entity -&gt; user group &amp; permission key value pair collection
/// </summary>
public class EntityPermissionSet
{
    private static readonly Lazy<EntityPermissionSet> EmptyInstance =
        new(() => new EntityPermissionSet(-1, new EntityPermissionCollection()));

    /// <summary>
    ///     Initializes a new instance of the <see cref="EntityPermissionSet" /> class.
    /// </summary>
    /// <param name="entityId">The entity identifier.</param>
    /// <param name="permissionsSet">The collection of entity permissions.</param>
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
    ///     The key/value pairs of user group id &amp; single permission
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
    public ISet<string> GetAllPermissions() => PermissionsSet.GetAllPermissions();
}
