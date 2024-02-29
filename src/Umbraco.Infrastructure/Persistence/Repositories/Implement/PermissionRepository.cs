using System.Globalization;
using Microsoft.Extensions.Logging;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Models.Membership.Permissions;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

/// <summary>
///     A (sub) repository that exposes functionality to modify assigned permissions to a node
/// </summary>
/// <typeparam name="TEntity"></typeparam>
/// <remarks>
///     This repo implements the base <see cref="EntityRepositoryBase{TId, TEntity}" /> class so that permissions can be
///     queued to be persisted
///     like the normal repository pattern but the standard repository Get commands don't apply and will throw
///     <see cref="NotImplementedException" />
/// </remarks>
internal class PermissionRepository<TEntity> : EntityRepositoryBase<int, ContentPermissionSet>
    where TEntity : class, IEntity
{
    public PermissionRepository(IScopeAccessor scopeAccessor, AppCaches cache, ILogger<PermissionRepository<TEntity>> logger)
        : base(scopeAccessor, cache, logger)
    {
    }

    /// <summary>
    ///     Returns explicitly defined permissions for a user group for any number of nodes
    /// </summary>
    /// <param name="userGroupIds">
    ///     The group ids to lookup permissions for
    /// </param>
    /// <param name="entityIds"></param>
    /// <returns></returns>
    /// <remarks>
    ///     This method will not support passing in more than 2000 group IDs when also passing in entity IDs.
    /// </remarks>
    public EntityPermissionCollection GetPermissionsForEntities(int[] userGroupIds, params int[] entityIds)
    {
        var result = new EntityPermissionCollection();

        if (entityIds.Length == 0)
        {
            foreach (IEnumerable<int> group in userGroupIds.InGroupsOf(Constants.Sql.MaxParameterCount))
            {
                Sql<ISqlContext> sql = Sql()
                    .Select<UserGroup2GranularPermissionDto>("gp").AndSelect("ug.id as userGroupId, en.id as entityId")
                    .From<UserGroupDto>("ug")
                    .InnerJoin<UserGroup2GranularPermissionDto>("gp")
                    .On<UserGroup2GranularPermissionDto, UserGroupDto>((left, right) => left.UserGroupKey == right.Key && group.Contains(right.Id), "gp", "ug")
                    .InnerJoin<NodeDto>("en")
                    .On<UserGroup2GranularPermissionDto, NodeDto>((left, right) => left.UniqueId == right.UniqueId, "gp", "en");

                List<UserGroup2GranularPermissionWithIdsDto> permissions =
                    AmbientScope.Database.Fetch<UserGroup2GranularPermissionWithIdsDto>(sql);

                foreach (EntityPermission permission in ConvertToPermissionList(permissions))
                {
                    result.Add(permission);
                }
            }
        }
        else
        {
            foreach (IEnumerable<int> entityGroup in entityIds.InGroupsOf(Constants.Sql.MaxParameterCount -
                                                                    userGroupIds.Length))
            {
                Sql<ISqlContext> sql = Sql()
                    .Select<UserGroup2GranularPermissionDto>("gp").AndSelect("ug.id as userGroupId, en.id as entityId")
                    .From<UserGroupDto>("ug")
                    .InnerJoin<UserGroup2GranularPermissionDto>("gp")
                    .On<UserGroup2GranularPermissionDto, UserGroupDto>((left, right) => left.UserGroupKey == right.Key && userGroupIds.Contains(right.Id), "gp", "ug")
                    .InnerJoin<NodeDto>("en")
                    .On<UserGroup2GranularPermissionDto, NodeDto>((left, right) => left.UniqueId == right.UniqueId, "gp", "en")
                    .Where<NodeDto>(en =>  entityGroup.Contains(en.NodeId), "en");

                List<UserGroup2GranularPermissionWithIdsDto> permissions =
                    AmbientScope.Database.Fetch<UserGroup2GranularPermissionWithIdsDto>(sql);

                foreach (EntityPermission permission in ConvertToPermissionList(permissions))
                {
                    result.Add(permission);
                }
            }
        }

        return result;
    }

    /// <summary>
    ///     Returns permissions directly assigned to the content items for all user groups
    /// </summary>
    /// <param name="entityIds"></param>
    /// <returns></returns>
    public IEnumerable<EntityPermission> GetPermissionsForEntities(int[] entityIds)
    {
        Sql<ISqlContext> sql = Sql()
            .Select<UserGroup2GranularPermissionDto>("gp").AndSelect("ug.id as userGroupId, en.id as entityId")
            .From<UserGroupDto>("ug")
            .InnerJoin<UserGroup2GranularPermissionDto>("gp")
            .On<UserGroup2GranularPermissionDto, UserGroupDto>((left, right) => left.UserGroupKey == right.Key, "gp", "ug")
            .InnerJoin<NodeDto>("en")
            .On<UserGroup2GranularPermissionDto, NodeDto>((left, right) => left.UniqueId == right.UniqueId, "gp", "en")
            .Where<NodeDto>(en =>  entityIds.Contains(en.NodeId), "en");

        List<UserGroup2GranularPermissionWithIdsDto> permissions =
            AmbientScope.Database.Fetch<UserGroup2GranularPermissionWithIdsDto>(sql);

        return ConvertToPermissionList(permissions);
    }

    /// <summary>
    ///     Returns permissions directly assigned to the content item for all user groups
    /// </summary>
    /// <param name="entityId"></param>
    /// <returns></returns>
    public EntityPermissionCollection GetPermissionsForEntity(int entityId)
    {
        Sql<ISqlContext> sql = Sql()
            .Select<UserGroup2GranularPermissionDto>("gp").AndSelect("ug.id as userGroupId, en.id as entityId")
            .From<UserGroupDto>("ug")
            .InnerJoin<UserGroup2GranularPermissionDto>("gp")
            .On<UserGroup2GranularPermissionDto, UserGroupDto>((left, right) => left.UserGroupKey == right.Key, "gp", "ug")
            .InnerJoin<NodeDto>("en")
            .On<UserGroup2GranularPermissionDto, NodeDto>((left, right) => left.UniqueId == right.UniqueId, "gp", "en")
            .Where<NodeDto>(en =>  entityId == en.NodeId, "en");

        List<UserGroup2GranularPermissionWithIdsDto> permissions =
            AmbientScope.Database.Fetch<UserGroup2GranularPermissionWithIdsDto>(sql);

        return ConvertToPermissionList(permissions);
    }

    /// <summary>
    ///     Assigns the same permission set for a single group to any number of entities
    /// </summary>
    /// <param name="groupId"></param>
    /// <param name="permissions">The permissions to assign or null to remove the connection between group and entityIds</param>
    /// <param name="entityIds"></param>
    /// <remarks>
    ///     This will first clear the permissions for this user and entities and recreate them
    /// </remarks>
    public void ReplacePermissions(int groupId, ISet<string> permissions, params int[] entityIds)
    {
        if (entityIds.Length == 0)
        {
            return;
        }

        IUmbracoDatabase db = AmbientScope.Database;

        db.Execute(
            Sql()
                .Delete<UserGroup2GranularPermissionDto>()
                .WhereIn<UserGroup2GranularPermissionDto>(
                    x => x.UniqueId,
                    Sql()
                        .Select<NodeDto>()
                        .From<NodeDto>()
                        .Where<NodeDto>(x => entityIds.Contains(x.NodeId)))
                .WhereIn<UserGroup2GranularPermissionDto>(
                    x => x.UserGroupKey,
                    Sql()
                        .Select<UserGroupDto>(x=>x.Key)
                        .From<UserGroupDto>()
                        .Where<UserGroupDto>(x => x.Id == groupId)));

        // This is a poor man's solution to avoid breaking changes.. Sooner or later we should obsolete this method and take Guids as input.
        Guid userGroupKey = db.Fetch<Guid>(Sql().Select<UserGroupDto>(x => x.Key).From<UserGroupDto>()
            .Where<UserGroupDto>(x => x.Id == groupId)).SingleOrDefault();
        var idToKey = db.Fetch<NodeDto>(Sql().Select<NodeDto>().From<NodeDto>()
            .Where<NodeDto>(x => entityIds.Contains(x.NodeId))).ToDictionary(x=>x.NodeId, x=>x.UniqueId);

        IEnumerable<UserGroup2GranularPermissionDto> toInsert =
            from entityId in entityIds
            from permission in permissions
            select new UserGroup2GranularPermissionDto()
            {
                Permission = permission, UniqueId = idToKey[entityId], UserGroupKey = userGroupKey, Context = DocumentGranularPermission.ContextType
            };

        db.InsertBulk(toInsert);
    }

    /// <summary>
    ///     Assigns one permission for a user to many entities
    /// </summary>
    /// <param name="groupId"></param>
    /// <param name="permission"></param>
    /// <param name="entityIds"></param>
    public void AssignPermission(int groupId, string permission, params int[] entityIds)
    {
        IUmbracoDatabase db = AmbientScope.Database;

        db.Execute(
            Sql()
                .Delete<UserGroup2GranularPermissionDto>()
                .Where<UserGroup2GranularPermissionDto>(x => x.Permission == permission)
                .WhereIn<UserGroup2GranularPermissionDto>(
                    x => x.UniqueId,
                    Sql()
                        .Select<NodeDto>()
                        .From<NodeDto>()
                        .Where<NodeDto>(x => entityIds.Contains(x.NodeId)))
                .WhereIn<UserGroup2GranularPermissionDto>(
                    x => x.UserGroupKey,
                    Sql()
                        .Select<UserGroupDto>(x=>x.Key)
                        .From<UserGroupDto>()
                        .Where<UserGroupDto>(x => x.Id == groupId)));

        // This is a poor man's solution to avoid breaking changes.. Sooner or later we should obsolete this method and take Guids as input.
        var userGroupKey = db.Fetch<Guid>(Sql().Select<UserGroupDto>(x => x.Key).From<UserGroupDto>()
            .Where<UserGroupDto>(x => x.Id == groupId)).SingleOrDefault();
        var idToKey = db.Fetch<NodeDto>(Sql().Select<NodeDto>().From<NodeDto>()
            .Where<NodeDto>(x => entityIds.Contains(x.NodeId))).ToDictionary(x=>x.NodeId, x=>x.UniqueId);

        var toInsert = entityIds.Select(e => new UserGroup2GranularPermissionDto()
                {
                    Permission = permission,
                    UniqueId = idToKey[e],
                    UserGroupKey = userGroupKey,
                    Context = DocumentGranularPermission.ContextType
                });

        db.InsertBulk(toInsert);
    }

    /// <summary>
    ///     Assigns one permission to an entity for multiple groups
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="permission"></param>
    /// <param name="groupIds"></param>
    public void AssignEntityPermission(TEntity entity, string permission, IEnumerable<int> groupIds)
    {
        IUmbracoDatabase db = AmbientScope.Database;

        db.Execute(
            Sql()
                .Delete<UserGroup2GranularPermissionDto>()
                .Where<UserGroup2GranularPermissionDto>(x => x.Permission == permission && x.UniqueId == entity.Key)
                .WhereIn<UserGroup2GranularPermissionDto>(
                    x => x.UserGroupKey,
                    Sql()
                        .Select<UserGroupDto>(x=>x.Key)
                        .From<UserGroupDto>()
                        .Where<UserGroupDto>(x => groupIds.Contains(x.Id))));

        // This is a poor man's solution to avoid breaking changes.. Sooner or later we should obsolete this method and take Guids as input.
        var idToKey = db.Fetch<UserGroupDto>(Sql().Select<UserGroupDto>().From<UserGroupDto>()
            .Where<UserGroupDto>(x => groupIds.Contains(x.Id))).ToDictionary(x=>x.Id, x=>x.Key);

        var toInsert = groupIds.Select(x => new UserGroup2GranularPermissionDto()
        {
            Permission = permission, UniqueId = entity.Key, UserGroupKey = idToKey[x], Context = DocumentGranularPermission.ContextType
        });

        db.InsertBulk(toInsert);

    }

    /// <summary>
    ///     Assigns permissions to an entity for multiple group/permission entries
    /// </summary>
    /// <param name="permissionSet">
    /// </param>
    /// <remarks>
    ///     This will first clear the permissions for this entity then re-create them
    /// </remarks>
    public void ReplaceEntityPermissions(EntityPermissionSet permissionSet)
    {
        IUmbracoDatabase db = AmbientScope.Database;

        db.Execute(
            Sql()
                .Delete<UserGroup2GranularPermissionDto>()
                .WhereIn<UserGroup2GranularPermissionDto>(
                    x => x.UniqueId,
                    Sql()
                        .Select<NodeDto>(x => x.UniqueId)
                        .From<NodeDto>()
                        .Where<NodeDto>(x => x.NodeId == permissionSet.EntityId)));

        // This is a poor man's solution to avoid breaking changes.. Sooner or later we should obsolete this method and take Guids as input.
        var userGroupIds = permissionSet.PermissionsSet.Select(x => x.UserGroupId);
        var entityKey = db.Fetch<Guid>(Sql().Select<NodeDto>(x => x.UniqueId).From<NodeDto>()
            .Where<NodeDto>(x => x.NodeId == permissionSet.EntityId)).SingleOrDefault();
        var idToKey = db.Fetch<UserGroupDto>(Sql().Select<UserGroupDto>().From<UserGroupDto>()
            .Where<UserGroupDto>(x => userGroupIds.Contains(x.Id))).ToDictionary(x=>x.Id, x=>x.Key);


        var toInsert = permissionSet.PermissionsSet
            .SelectMany(x => x.AssignedPermissions
                .Select(p => new UserGroup2GranularPermissionDto()
                {
                    Permission = p,
                    UniqueId = entityKey,
                    UserGroupKey = idToKey[x.UserGroupId],
                    Context = DocumentGranularPermission.ContextType
                }));

        db.InsertBulk(toInsert);
    }

    /// <summary>
    ///     Used to add or update entity permissions during a content item being updated
    /// </summary>
    /// <param name="entity"></param>
    protected override void PersistNewItem(ContentPermissionSet entity) =>

        // Does the same thing as update
        PersistUpdatedItem(entity);

    /// <summary>
    ///     Used to add or update entity permissions during a content item being updated
    /// </summary>
    /// <param name="entity"></param>
    protected override void PersistUpdatedItem(ContentPermissionSet entity)
    {
        var asIEntity = (IEntity)entity;
        if (asIEntity.HasIdentity == false)
        {
            throw new InvalidOperationException("Cannot create permissions for an entity without an Id");
        }

        ReplaceEntityPermissions(entity);
    }


    private static EntityPermissionCollection ConvertToPermissionList(
        IEnumerable<UserGroup2GranularPermissionWithIdsDto> result)
    {
        var permissions = new EntityPermissionCollection();
        IEnumerable<IGrouping<int, UserGroup2GranularPermissionWithIdsDto>> nodePermissions = result.GroupBy(x => x.EntityId).OrderBy(x=>x.Key);
        foreach (IGrouping<int, UserGroup2GranularPermissionWithIdsDto> np in nodePermissions)
        {
            IEnumerable<IGrouping<int, UserGroup2GranularPermissionWithIdsDto>> userGroupPermissions =
                np.GroupBy(x => x.UserGroupId);
            foreach (IGrouping<int, UserGroup2GranularPermissionWithIdsDto> permission in userGroupPermissions)
            {
                var perms = permission.Select(x => x.Permission).Distinct().WhereNotNull().ToHashSet();

                // perms can contain null if there are no permissions assigned, but the node is chosen in the UI.
                permissions.Add(new EntityPermission(permission.Key, np.Key, perms));
            }
        }

        return permissions;
    }

    #region Not implemented (don't need to for the purposes of this repo)

    protected override ContentPermissionSet PerformGet(int id) =>
        throw new InvalidOperationException("This method won't be implemented.");

    protected override IEnumerable<ContentPermissionSet> PerformGetAll(params int[]? ids) =>
        throw new InvalidOperationException("This method won't be implemented.");

    protected override IEnumerable<ContentPermissionSet> PerformGetByQuery(IQuery<ContentPermissionSet> query) =>
        throw new InvalidOperationException("This method won't be implemented.");

    protected override Sql<ISqlContext> GetBaseQuery(bool isCount) =>
        throw new InvalidOperationException("This method won't be implemented.");

    protected override string GetBaseWhereClause() =>
        throw new InvalidOperationException("This method won't be implemented.");

    protected override IEnumerable<string> GetDeleteClauses() => new List<string>();

    protected override void PersistDeletedItem(ContentPermissionSet entity) =>
        throw new InvalidOperationException("This method won't be implemented.");

    #endregion
}
