namespace Umbraco.Cms.Core.Models.Membership;

/// <summary>
///     A <see cref="HashSet{T}" /> of <see cref="EntityPermission" />
/// </summary>
public class EntityPermissionCollection : HashSet<EntityPermission>
{
    private Dictionary<int, ISet<string>>? _aggregateNodePermissions;

    private ISet<string>? _aggregatePermissions;

    public EntityPermissionCollection()
    {
    }

    public EntityPermissionCollection(IEnumerable<EntityPermission> collection)
        : base(collection)
    {
    }

    /// <summary>
    ///     Returns the aggregate permissions in the permission set for a single node
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    ///     This value is only calculated once per node
    /// </remarks>
    public ISet<string> GetAllPermissions(int entityId)
    {
        if (_aggregateNodePermissions == null)
        {
            _aggregateNodePermissions = new Dictionary<int, ISet<string>>();
        }

        if (_aggregateNodePermissions.TryGetValue(entityId, out ISet<string>? entityPermissions) == false)
        {
            entityPermissions = this.Where(x => x.EntityId == entityId).SelectMany(x => x.AssignedPermissions)
                .ToHashSet();
            _aggregateNodePermissions[entityId] = entityPermissions;
        }

        return entityPermissions;
    }

    /// <summary>
    ///     Returns the aggregate permissions in the permission set for all nodes
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    ///     This value is only calculated once
    /// </remarks>
    public ISet<string> GetAllPermissions() =>
_aggregatePermissions ??=
            this.SelectMany(x => x.AssignedPermissions).Distinct().ToHashSet();
}
