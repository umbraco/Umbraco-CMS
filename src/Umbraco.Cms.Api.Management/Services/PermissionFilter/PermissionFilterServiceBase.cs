using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services.OperationStatus;

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
    protected abstract string BrowseActionLetter { get; }

    /// <summary>
    /// Filters entities based on the current user's browse permissions.
    /// </summary>
    /// <param name="entities">The entities to filter.</param>
    /// <param name="totalItems">The total number of items before filtering.</param>
    /// <returns>A tuple containing the filtered entities and the adjusted total items count.</returns>
    public async Task<(IEntitySlim[] Entities, long TotalItems)> FilterAsync(IEntitySlim[] entities, long totalItems)
    {
        Dictionary<Guid, NodePermissions>? permissionsByNodeKey = await GetPermissionsByNodeKeyAsync(entities);
        if (permissionsByNodeKey is null)
        {
            return (entities, totalItems);
        }

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
        Dictionary<Guid, NodePermissions>? permissionsByNodeKey = await GetPermissionsByNodeKeyAsync(entities);
        if (permissionsByNodeKey is null)
        {
            return (entities, totalBefore, totalAfter);
        }

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
    /// <param name="userKey">The key of the user to retrieve permissions for.</param>
    /// <param name="entityKeys">The keys of the entities to retrieve permissions for.</param>
    /// <returns>An attempt containing the node permissions or a failure status.</returns>
    protected abstract Task<Attempt<IEnumerable<NodePermissions>, UserOperationStatus>> GetPermissionsAsync(
        Guid userKey,
        HashSet<Guid> entityKeys);

    private async Task<Dictionary<Guid, NodePermissions>?> GetPermissionsByNodeKeyAsync(IEntitySlim[] entities)
    {
        Guid userKey = CurrentUserKey();
        var entityKeys = entities.Select(e => e.Key).ToHashSet();

        Attempt<IEnumerable<NodePermissions>, UserOperationStatus> permissionsAttempt =
            await GetPermissionsAsync(userKey, entityKeys);

        return permissionsAttempt.Success
            ? permissionsAttempt.Result.ToDictionary(p => p.NodeKey)
            : null;
    }

    private Guid CurrentUserKey()
        => _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser?.Key
           ?? throw new InvalidOperationException("No backoffice user found");

    private IEntitySlim[] FilterEntitiesWithBrowsePermission(IEntitySlim[] entities, Dictionary<Guid, NodePermissions> permissionsByNodeKey)
        => entities.Where(e => HasBrowsePermission(e, permissionsByNodeKey)).ToArray();

    private bool HasBrowsePermission(IEntitySlim entity, Dictionary<Guid, NodePermissions> permissionsByNodeKey)
        => permissionsByNodeKey.TryGetValue(entity.Key, out NodePermissions? nodePermissions) is false
           || nodePermissions.Permissions.Contains(BrowseActionLetter);
}
