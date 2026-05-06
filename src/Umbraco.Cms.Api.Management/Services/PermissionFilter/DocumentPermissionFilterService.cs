using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Services.PermissionFilter;

/// <summary>
/// Provides functionality to filter document entities based on the current user's permissions.
/// </summary>
internal sealed class DocumentPermissionFilterService : IDocumentPermissionFilterService
{
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IContentPermissionService _contentPermissionService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentPermissionFilterService"/> class.
    /// </summary>
    /// <param name="backOfficeSecurityAccessor">Provides access to the current backoffice user's security context.</param>
    /// <param name="contentPermissionService">Service used to retrieve content permissions.</param>
    public DocumentPermissionFilterService(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IContentPermissionService contentPermissionService)
    {
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _contentPermissionService = contentPermissionService;
    }

    /// <inheritdoc />
    public async Task<(IEntitySlim[] Entities, long TotalItems)> FilterAsync(IEntitySlim[] entities, long totalItems)
    {
        Dictionary<Guid, NodePermissions> permissionsByNodeKey = await GetDocumentPermissionsByNodeKeyAsync(entities);

        IEntitySlim[] filteredEntities = FilterEntitiesWithBrowsePermission(entities, permissionsByNodeKey);
        var removedCount = entities.Length - filteredEntities.Length;

        return (filteredEntities, totalItems - removedCount);
    }

    /// <inheritdoc />
    public async Task<(IEntitySlim[] Entities, long TotalBefore, long TotalAfter)> FilterAsync(Guid targetKey, IEntitySlim[] entities, long totalBefore, long totalAfter)
    {
        Dictionary<Guid, NodePermissions> permissionsByNodeKey = await GetDocumentPermissionsByNodeKeyAsync(entities);

        // Find the index of the target entity to determine before/after boundaries
        var targetIndex = Array.FindIndex(entities, e => e.Key == targetKey);

        // Count removed entities before and after the target separately
        var removedBefore = entities.Take(targetIndex).Count(e => HasBrowsePermission(e, permissionsByNodeKey) is false);
        var removedAfter = entities.Skip(targetIndex + 1).Count(e => HasBrowsePermission(e, permissionsByNodeKey) is false);

        IEntitySlim[] filteredEntities = FilterEntitiesWithBrowsePermission(entities, permissionsByNodeKey);

        return (filteredEntities, totalBefore - removedBefore, totalAfter - removedAfter);
    }

    private async Task<Dictionary<Guid, NodePermissions>> GetDocumentPermissionsByNodeKeyAsync(IEntitySlim[] entities)
    {
        IUser currentUser = _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser
                            ?? throw new InvalidOperationException("No backoffice user found");

        var entityKeys = entities.Select(e => e.Key).ToHashSet();

        IEnumerable<NodePermissions> permissions = await _contentPermissionService.GetPermissionsAsync(currentUser, entityKeys);

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

    private static IEntitySlim[] FilterEntitiesWithBrowsePermission(IEntitySlim[] entities, Dictionary<Guid, NodePermissions> permissionsByNodeKey)
        => entities.Where(e => HasBrowsePermission(e, permissionsByNodeKey)).ToArray();

    private static bool HasBrowsePermission(IEntitySlim entity, Dictionary<Guid, NodePermissions> permissionsByNodeKey)
        => permissionsByNodeKey.TryGetValue(entity.Key, out NodePermissions? nodePermissions)
           && nodePermissions.Permissions.Contains(ActionBrowse.ActionLetter);
}
