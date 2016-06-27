using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Umbraco.Core.Cache;
using Umbraco.Core.Events;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Persistence.UnitOfWork;
using CacheKeys = Umbraco.Core.Cache.CacheKeys;


namespace Umbraco.Core.Persistence.Repositories
{
    /// <summary>
    /// A repository that exposes functionality to modify assigned permissions to a node
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    internal class GroupPermissionRepository<TEntity> : PermissionRepositoryBase<TEntity>
        where TEntity : class, IAggregateRoot
    {
        private readonly IRuntimeCacheProvider _runtimeCache;

        internal GroupPermissionRepository(IDatabaseUnitOfWork unitOfWork, CacheHelper cache, ISqlSyntaxProvider sqlSyntax)
            :base(unitOfWork, sqlSyntax)
        {
            //Make this repository use an isolated cache
            _runtimeCache = cache.IsolatedRuntimeCache.GetOrCreateCache<GroupEntityPermission>();
        }

        /// <summary>
        /// Returns permissions for a given group for any number of nodes
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="entityIds"></param>
        /// <returns></returns>        
        public IEnumerable<GroupEntityPermission> GetGroupPermissionsForEntities(int groupId, params int[] entityIds)
        {
            var entityIdKey = GetEntityIdKey(entityIds);
            return _runtimeCache.GetCacheItem<IEnumerable<GroupEntityPermission>>(
                string.Format("{0}{1}{2}",
                    CacheKeys.UserGroupPermissionsCacheKey,
                    groupId,
                    entityIdKey),
                () =>
                {
                    var whereCriteria = GetPermissionsForEntitiesCriteria(groupId, false, entityIds);
                    var sql = new Sql();
                    sql.Select("*")
                        .From<UserGroup2NodePermissionDto>()
                        .Where(whereCriteria);
                    var result = UnitOfWork.Database.Fetch<UserGroup2NodePermissionDto>(sql).ToArray(); // ToArray() to ensure it's all fetched from the db once
                    return ConvertToPermissionList(result);
                },
                GetCacheTimeout(),
                priority: GetCachePriority());
        }

        /// <summary>
        /// Returns permissions for all groups for a given entity
        /// </summary>
        /// <param name="entityId"></param>
        /// <returns></returns>
        public IEnumerable<GroupEntityPermission> GetPermissionsForEntity(int entityId)
        {
            var sql = new Sql();
            sql.Select("*")
                .From<UserGroup2NodePermissionDto>()
                .Where<UserGroup2NodePermissionDto>(dto => dto.NodeId == entityId)
                .OrderBy<UserGroup2NodePermissionDto>(dto => dto.NodeId);

            var result = UnitOfWork.Database.Fetch<UserGroup2NodePermissionDto>(sql).ToArray(); // ToArray() to ensure it's all fetched from the db once
            return ConvertToPermissionList(result);
        }

        /// <summary>
        /// Assigns the same permission set for a single group to any number of entities
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="permissions"></param>
        /// <param name="entityIds"></param>
        /// <remarks>
        /// This will first clear the permissions for this user and entities and recreate them
        /// </remarks>
        public void ReplaceGroupPermissions(int groupId, IEnumerable<char> permissions, params int[] entityIds)
        {
            var db = UnitOfWork.Database;
            using (var trans = db.GetTransaction())
            {
                //we need to batch these in groups of 2000 so we don't exceed the max 2100 limit
                var tableName = GetTableName();
                var fieldName = GetFieldName();
                var sql = string.Format("DELETE FROM {0} WHERE {1} = @groupId AND nodeId in (@nodeIds)",
                    tableName, fieldName);
                foreach (var idGroup in entityIds.InGroupsOf(2000))
                {
                    db.Execute(sql, new { groupId = groupId, nodeIds = idGroup });
                }

                var toInsert = new List<UserGroup2NodePermissionDto>();
                foreach (var p in permissions)
                {
                    foreach (var e in entityIds)
                    {
                        toInsert.Add(new UserGroup2NodePermissionDto
                        {
                            NodeId = e,
                            Permission = p.ToString(CultureInfo.InvariantCulture),
                            UserGroupId = groupId
                        });
                    }
                }

                UnitOfWork.Database.BulkInsertRecords(toInsert, trans);
                trans.Complete();

                AssignedPermissions.RaiseEvent(new SaveEventArgs<GroupEntityPermission>(ConvertToPermissionList(toInsert), false), this);
            }
        }

        /// <summary>
        /// Assigns one permission for a user to many entities
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="permission"></param>
        /// <param name="entityIds"></param>
        public void AssignGroupPermission(int groupId, char permission, params int[] entityIds)
        {
            var db = UnitOfWork.Database;
            using (var trans = db.GetTransaction())
            {
                var tableName = GetTableName();
                var fieldName = GetFieldName();
                var sql = String.Format("DELETE FROM {0} WHERE {1} = @groupId AND permission=@permission AND nodeId in (@entityIds)",
                    tableName, fieldName);
                db.Execute(sql,
                    new
                    {
                        groupId = groupId,
                        permission = permission.ToString(CultureInfo.InvariantCulture),
                        entityIds = entityIds
                    });

                var actions = entityIds.Select(id => new UserGroup2NodePermissionDto
                {
                    NodeId = id,
                    Permission = permission.ToString(CultureInfo.InvariantCulture),
                    UserGroupId = groupId
                }).ToArray();

                UnitOfWork.Database.BulkInsertRecords(actions, trans);
                trans.Complete();

                AssignedPermissions.RaiseEvent(new SaveEventArgs<GroupEntityPermission>(ConvertToPermissionList(actions), false), this);
            }
        }

        /// <summary>
        /// Assigns one permission to an entity for multiple groups
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="permission"></param>
        /// <param name="groupIds"></param>
        public void AssignEntityPermission(TEntity entity, char permission, IEnumerable<int> groupIds)
        {
            var db = UnitOfWork.Database;
            using (var trans = db.GetTransaction())
            {
                var tableName = GetTableName();
                var fieldName = GetFieldName();
                var sql = string.Format("DELETE FROM {0} WHERE nodeId = @nodeId AND permission = @permission AND {1} in (@groupIds)",
                    tableName, fieldName);
                db.Execute(sql,
                    new
                    {
                        nodeId = entity.Id,
                        permission = permission.ToString(CultureInfo.InvariantCulture),
                        groupIds = groupIds
                    });

                var actions = groupIds.Select(id => new UserGroup2NodePermissionDto
                {
                    NodeId = entity.Id,
                    Permission = permission.ToString(CultureInfo.InvariantCulture),
                    UserGroupId = id
                }).ToArray();

                UnitOfWork.Database.BulkInsertRecords(actions, trans);
                trans.Complete();

                AssignedPermissions.RaiseEvent(new SaveEventArgs<GroupEntityPermission>(ConvertToPermissionList(actions), false), this);
            }
        }

        /// <summary>
        /// Assigns permissions to an entity for multiple users/permission entries
        /// </summary>
        /// <param name="permissionSet">
        /// </param>
        /// <remarks>
        /// This will first clear the permissions for this entity then re-create them
        /// </remarks>
        public void ReplaceGroupPermissions(GroupEntityPermissionSet permissionSet)
        {
            var db = UnitOfWork.Database;
            using (var trans = db.GetTransaction())
            {
                var tableName = GetTableName();
                var sql = String.Format("DELETE FROM {0} WHERE nodeId = @nodeId", tableName);
                db.Execute(sql, new { nodeId = permissionSet.EntityId });

                var actions = permissionSet.PermissionsSet.Select(p => new UserGroup2NodePermissionDto
                {
                    NodeId = permissionSet.EntityId,
                    Permission = p.Permission,
                    UserGroupId = p.GroupId
                }).ToArray();

                UnitOfWork.Database.BulkInsertRecords(actions, trans);
                trans.Complete();

                AssignedPermissions.RaiseEvent(new SaveEventArgs<GroupEntityPermission>(ConvertToPermissionList(actions), false), this);
            }
        }

        protected override string GetFieldName()
        {
            return "userGroupId";
        }

        protected override string GetTableName()
        {
            return "umbracoUserGroup2NodePermission";
        }

        public static event TypedEventHandler<GroupPermissionRepository<TEntity>, SaveEventArgs<GroupEntityPermission>> AssignedPermissions;
    }
}