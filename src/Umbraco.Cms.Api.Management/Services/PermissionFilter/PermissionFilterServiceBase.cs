using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;

namespace Umbraco.Cms.Api.Management.Services.PermissionFilter;

/// <summary>
/// Base class for filtering entities based on the current user's browse permissions.
/// </summary>
internal abstract class PermissionFilterServiceBase
{
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="PermissionFilterServiceBase"/> class.
    /// </summary>
    /// <param name="backOfficeSecurityAccessor">Provides access to the current backoffice user's security context.</param>
    protected PermissionFilterServiceBase(IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
        => _backOfficeSecurityAccessor = backOfficeSecurityAccessor;

    /// <summary>
    /// Gets the browse action letter used to check permissions.
    /// </summary>
    protected abstract string BrowseActionLetter(IEntitySlim entity);

    /// <summary>
    /// Filters entities based on the current user's browse permissions.
    /// </summary>
    /// <param name="entities">The entities to filter.</param>
    /// <param name="totalItems">The total number of items before filtering.</param>
    /// <returns>A tuple containing the filtered entities and the adjusted total items count.</returns>
    public async Task<(IEntitySlim[] Entities, long TotalItems)> FilterAsync(IEntitySlim[] entities, long totalItems)
    {
        Dictionary<Guid, NodePermissions> permissionsByNodeKey = await GetPermissionsByNodeKeyAsync(entities);

        IEntitySlim[] filteredEntities = FilterEntitiesWithBrowsePermission(entities, permissionsByNodeKey);
        var removedCount = entities.Length - filteredEntities.Length;

        return (filteredEntities, totalItems - removedCount);
    }

    /// <summary>
    /// Filters sibling entities based on the current user's browse permissions.
    /// </summary>
    /// <param name="targetKey">The key of the target entity around which siblings are being retrieved.</param>
    /// <param name="entities">The entities to filter.</param>
    /// <param name="totalBefore">The total number of siblings before the target entity.</param>
    /// <param name="totalAfter">The total number of siblings after the target entity.</param>
    /// <returns>A tuple containing the filtered entities and the adjusted before/after counts.</returns>
    public async Task<(IEntitySlim[] Entities, long TotalBefore, long TotalAfter)> FilterAsync(
        Guid targetKey,
        IEntitySlim[] entities,
        long totalBefore,
        long totalAfter)
    {
        Dictionary<Guid, NodePermissions> permissionsByNodeKey = await GetPermissionsByNodeKeyAsync(entities);

        // Find the index of the target entity to determine before/after boundaries
        var targetIndex = Array.FindIndex(entities, e => e.Key == targetKey);

        // Count removed entities before and after the target separately
        var removedBefore = entities.Take(targetIndex).Count(e => HasBrowsePermission(e, permissionsByNodeKey) is false);
        var removedAfter = entities.Skip(targetIndex + 1).Count(e => HasBrowsePermission(e, permissionsByNodeKey) is false);

        IEntitySlim[] filteredEntities = FilterEntitiesWithBrowsePermission(entities, permissionsByNodeKey);

        return (filteredEntities, totalBefore - removedBefore, totalAfter - removedAfter);
    }

    /// <summary>
    /// Retrieves node permissions for the given user and entity keys.
    /// </summary>
    /// <param name="user">The current backoffice user to retrieve permissions for.</param>
    /// <param name="entityKeys">The keys of the entities to retrieve permissions for.</param>
    /// <returns>The effective permissions for each entity. Entries missing from the result are treated as denied (fail-closed).</returns>
    protected abstract Task<IEnumerable<NodePermissions>> GetPermissionsAsync(
        IUser user,
        IEnumerable<Guid> entityKeys);

    private async Task<Dictionary<Guid, NodePermissions>> GetPermissionsByNodeKeyAsync(IEntitySlim[] entities)
    {
        IUser currentUser = _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser
                            ?? throw new InvalidOperationException("No backoffice user found");

        var entityKeys = entities.Select(e => e.Key).ToHashSet();

        IEnumerable<NodePermissions> permissions = await GetPermissionsAsync(currentUser, entityKeys);

        // Build dictionary with an entry for every requested key. Keys missing from the result
        // default to empty permissions so they are treated as denied (fail-closed).
        var result = entityKeys.ToDictionary(
            key => key,
            key => new NodePermissions { NodeKey = key, Permissions = new HashSet<string>() });

        foreach (NodePermissions nodePermissions in permissions)
        {
            result[nodePermissions.NodeKey] = nodePermissions;
        }

        return result;
    }

    private IEntitySlim[] FilterEntitiesWithBrowsePermission(IEntitySlim[] entities, Dictionary<Guid, NodePermissions> permissionsByNodeKey)
        => entities.Where(e => HasBrowsePermission(e, permissionsByNodeKey)).ToArray();

    private bool HasBrowsePermission(IEntitySlim entity, Dictionary<Guid, NodePermissions> permissionsByNodeKey)
        => permissionsByNodeKey.TryGetValue(entity.Key, out NodePermissions? nodePermissions)
           && nodePermissions.Permissions.Contains(BrowseActionLetter(entity));
}
