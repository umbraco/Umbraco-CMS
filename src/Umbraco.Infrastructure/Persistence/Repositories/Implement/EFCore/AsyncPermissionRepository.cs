using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Models.Membership.Permissions;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement.EFCore;

/// <summary>
///     A (sub) repository that exposes functionality to modify assigned granular permissions for a node,
///     backed by EF Core.
/// </summary>
/// <remarks>
///     Mirrors the NPoco <c>PermissionRepository{TEntity}</c> sub-repository — manually constructed by the
///     owning repository (e.g. <see cref="DocumentRepository" />), not DI-registered.
/// </remarks>
internal sealed class AsyncPermissionRepository<TEntity> : AsyncRepositoryBase
    where TEntity : class, IEntity
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="AsyncPermissionRepository{TEntity}" /> class.
    /// </summary>
    /// <param name="scopeAccessor">The EF Core scope accessor.</param>
    /// <param name="appCaches">The application caches.</param>
    public AsyncPermissionRepository(
        IEFCoreScopeAccessor<UmbracoDbContext> scopeAccessor,
        AppCaches appCaches)
        : base(scopeAccessor, appCaches)
    {
    }

    /// <summary>
    ///     Returns permissions directly assigned to the content item for all user groups.
    /// </summary>
    public Task<EntityPermissionCollection> GetPermissionsForEntityAsync(Guid entityKey, CancellationToken cancellationToken) =>
        AmbientScope.ExecuteWithContextAsync(async db =>
        {
            int entityId = await db.Nodes
                .Where(node => node.UniqueId == entityKey)
                .Select(node => node.NodeId)
                .SingleOrDefaultAsync(cancellationToken);
            if (entityId == 0)
            {
                return new EntityPermissionCollection();
            }

            List<(Guid UserGroupKey, string? Permission)> rows = await db.UserGroup2GranularPermissions
                .Where(permission => permission.UniqueId == entityKey)
                .Select(permission => new ValueTuple<Guid, string?>(permission.UserGroupKey, permission.Permission))
                .ToListAsync(cancellationToken);
            if (rows.Count == 0)
            {
                return new EntityPermissionCollection();
            }

            List<Guid> groupKeys = rows.Select(row => row.UserGroupKey).Distinct().ToList();
            Dictionary<Guid, int> keyToId = await db.UserGroups
                .Where(userGroup => groupKeys.Contains(userGroup.Key))
                .ToDictionaryAsync(userGroup => userGroup.Key, userGroup => userGroup.Id, cancellationToken);

            var collection = new EntityPermissionCollection();
            foreach (IGrouping<Guid, (Guid UserGroupKey, string? Permission)> group in rows.GroupBy(row => row.UserGroupKey))
            {
                if (keyToId.TryGetValue(group.Key, out int userGroupId))
                {
                    ISet<string> assignedPermissions = group.Select(row => row.Permission).WhereNotNull().Distinct().ToHashSet();
                    collection.Add(new EntityPermission(userGroupId, entityId, assignedPermissions));
                }
            }

            return collection;
        });

    /// <summary>
    ///     Assigns one permission to an entity for multiple groups.
    /// </summary>
    public Task AssignEntityPermissionAsync(TEntity entity, string permission, IEnumerable<Guid> groupKeys, CancellationToken cancellationToken) =>
        AmbientScope.ExecuteWithContextAsync<object>(async db =>
        {
            List<Guid> groupKeyList = groupKeys.ToList();

            await db.UserGroup2GranularPermissions
                .Where(granularPermission => granularPermission.Permission == permission && granularPermission.UniqueId == entity.Key && groupKeyList.Contains(granularPermission.UserGroupKey))
                .ExecuteDeleteAsync(cancellationToken);

            db.UserGroup2GranularPermissions.AddRange(groupKeyList.Select(groupKey => new UserGroup2GranularPermissionDto
            {
                Permission = permission,
                UniqueId = entity.Key,
                UserGroupKey = groupKey,
                Context = DocumentGranularPermission.ContextType,
            }));

            await db.SaveChangesAsync(cancellationToken);
        });

    /// <summary>
    ///     Assigns permissions to an entity for multiple group/permission entries.
    /// </summary>
    /// <remarks>
    ///     This will first clear the permissions for this entity then re-create them.
    /// </remarks>
    public Task ReplaceEntityPermissionsAsync(EntityPermissionSet permissionSet, CancellationToken cancellationToken) =>
        AmbientScope.ExecuteWithContextAsync<object>(db => ReplaceEntityPermissionsCoreAsync(db, permissionSet, cancellationToken));

    /// <summary>
    ///     Used to add or update entity permissions for a content item.
    /// </summary>
    public Task AddOrUpdatePermissionsAsync(ContentPermissionSet permission, CancellationToken cancellationToken)
    {
        // Both the insert and update paths end up here regardless of HasIdentity, because the insert path
        // forwards to this one.
        if (((IEntity)permission).HasIdentity == false)
        {
            throw new InvalidOperationException("Cannot create permissions for an entity without an Id");
        }

        return AmbientScope.ExecuteWithContextAsync<object>(db => ReplaceEntityPermissionsCoreAsync(db, permission, cancellationToken));
    }

    private async Task ReplaceEntityPermissionsCoreAsync(UmbracoDbContext db, EntityPermissionSet permissionSet, CancellationToken cancellationToken)
    {
        Guid entityKey = await db.Nodes
            .Where(node => node.NodeId == permissionSet.EntityId)
            .Select(node => node.UniqueId)
            .SingleAsync(cancellationToken);

        await db.UserGroup2GranularPermissions.Where(granularPermission => granularPermission.UniqueId == entityKey).ExecuteDeleteAsync(cancellationToken);

        List<int> groupIds = permissionSet.PermissionsSet.Select(entityPermission => entityPermission.UserGroupId).Distinct().ToList();
        Dictionary<int, Guid> idToKey = await db.UserGroups
            .Where(userGroup => groupIds.Contains(userGroup.Id))
            .ToDictionaryAsync(userGroup => userGroup.Id, userGroup => userGroup.Key, cancellationToken);

        db.UserGroup2GranularPermissions.AddRange(permissionSet.PermissionsSet.SelectMany(entityPermission =>
            entityPermission.AssignedPermissions.Select(assignedPermission => new UserGroup2GranularPermissionDto
            {
                Permission = assignedPermission,
                UniqueId = entityKey,
                UserGroupKey = idToKey[entityPermission.UserGroupId],
                Context = DocumentGranularPermission.ContextType,
            })));

        await db.SaveChangesAsync(cancellationToken);
    }
}
