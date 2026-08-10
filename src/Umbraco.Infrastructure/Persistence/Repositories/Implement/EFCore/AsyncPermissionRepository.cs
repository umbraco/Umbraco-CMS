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
///     owning repository (e.g. <see cref="AsyncDocumentRepository" />), not DI-registered.
/// </remarks>
internal sealed class AsyncPermissionRepository<TEntity> : AsyncRepositoryBase
    where TEntity : class, IEntity
{
    private readonly Lazy<IUserGroupService> _userGroupService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AsyncPermissionRepository{TEntity}" /> class.
    /// </summary>
    /// <param name="scopeAccessor">The EF Core scope accessor.</param>
    /// <param name="appCaches">The application caches.</param>
    /// <param name="userGroupService">
    ///     The user group service, used to resolve user group keys to/from IDs. Resolved lazily: <see cref="IUserGroupService" />
    ///     depends (transitively, via <see cref="IUserGroupPermissionService" />) on <see cref="IContentService" />, which owns
    ///     the document repository this class is a sub-repository of — resolving it eagerly at construction time closes a
    ///     circular dependency.
    /// </param>
    public AsyncPermissionRepository(
        IEFCoreScopeAccessor<UmbracoDbContext> scopeAccessor,
        AppCaches appCaches,
        Lazy<IUserGroupService> userGroupService)
        : base(scopeAccessor, appCaches) =>
        _userGroupService = userGroupService;

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
            Dictionary<Guid, int> keyToId = (await _userGroupService.Value.GetAsync(groupKeys))
                .ToDictionary(userGroup => userGroup.Key, userGroup => userGroup.Id);

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
                .Where(p => p.Permission == permission && p.UniqueId == entity.Key && groupKeyList.Contains(p.UserGroupKey))
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
        // Mirrors NPoco's PermissionRepository.PersistUpdatedItem guard — both PersistNewItem and
        // PersistUpdatedItem end up here regardless of HasIdentity, since PersistNewItem just forwards
        // to PersistUpdatedItem in the NPoco version too.
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

        await db.UserGroup2GranularPermissions.Where(p => p.UniqueId == entityKey).ExecuteDeleteAsync(cancellationToken);

        List<int> groupIds = permissionSet.PermissionsSet.Select(p => p.UserGroupId).Distinct().ToList();
        Dictionary<int, Guid> idToKey = (await _userGroupService.Value.GetAsync(groupIds.ToArray()))
            .ToDictionary(userGroup => userGroup.Id, userGroup => userGroup.Key);

        db.UserGroup2GranularPermissions.AddRange(permissionSet.PermissionsSet.SelectMany(p =>
            p.AssignedPermissions.Select(assignedPermission => new UserGroup2GranularPermissionDto
            {
                Permission = assignedPermission,
                UniqueId = entityKey,
                UserGroupKey = idToKey[p.UserGroupId],
                Context = DocumentGranularPermission.ContextType,
            })));

        await db.SaveChangesAsync(cancellationToken);
    }
}
