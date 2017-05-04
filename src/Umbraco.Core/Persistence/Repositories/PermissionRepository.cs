using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web.Caching;
using Umbraco.Core.Events;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Services;
using CacheKeys = Umbraco.Core.Cache.CacheKeys;
using Umbraco.Core.Cache;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    /// <summary>
    /// A repository that exposes functionality to modify assigned permissions to a node
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    internal class PermissionRepository<TEntity>
        where TEntity : class, IAggregateRoot
    {
        private readonly IScopeUnitOfWork _unitOfWork;
        private readonly IRuntimeCacheProvider _runtimeCache;
        private readonly ISqlSyntaxProvider _sqlSyntax;

        internal PermissionRepository(IScopeUnitOfWork unitOfWork, CacheHelper cache, ISqlSyntaxProvider sqlSyntax)
        {
            _unitOfWork = unitOfWork;
            //Make this repository use an isolated cache
            _runtimeCache = cache.IsolatedRuntimeCache.GetOrCreateCache<EntityPermission>();
            _sqlSyntax = sqlSyntax;
        }

        /// <summary>
        /// Returns permissions for a given group for any number of nodes
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="entityIds"></param>
        /// <returns></returns>        
        public IEnumerable<EntityPermission> GetPermissionsForEntities(int groupId, params int[] entityIds)
        {
            var entityIdKey = GetEntityIdKey(entityIds);
            return _runtimeCache.GetCacheItem<IEnumerable<EntityPermission>>(
                string.Format("{0}{1}{2}",
                    CacheKeys.UserGroupPermissionsCacheKey,
                    groupId,
                    entityIdKey),
                () =>
                {
                    var whereCriteria = GetPermissionsForEntitiesCriteria(groupId, entityIds);
                    var sql = new Sql();
                    sql.Select("*")
                        .From<UserGroup2NodePermissionDto>()
                        .Where(whereCriteria);
                    var result = _unitOfWork.Database.Fetch<UserGroup2NodePermissionDto>(sql).ToArray();
                        // ToArray() to ensure it's all fetched from the db once
                    return ConvertToPermissionList(result);
                },
                GetCacheTimeout(),
                priority: GetCachePriority());
        }

        private static string GetEntityIdKey(int[] entityIds)
        {
            return string.Join(",", entityIds.Select(x => x.ToString(CultureInfo.InvariantCulture)));
        }

        private string GetPermissionsForEntitiesCriteria(int groupId, params int[] entityIds)
        {
            var whereBuilder = new StringBuilder();
            whereBuilder.Append(_sqlSyntax.GetQuotedColumnName("userGroupId"));
            whereBuilder.Append("=");
            whereBuilder.Append(groupId);

            if (entityIds.Any())
            {
                whereBuilder.Append(" AND ");

                //where nodeId = @nodeId1 OR nodeId = @nodeId2, etc...
                whereBuilder.Append("(");
                for (var index = 0; index < entityIds.Length; index++)
                {
                    var entityId = entityIds[index];
                    whereBuilder.Append(_sqlSyntax.GetQuotedColumnName("nodeId"));
                    whereBuilder.Append("=");
                    whereBuilder.Append(entityId);
                    if (index < entityIds.Length - 1)
                    {
                        whereBuilder.Append(" OR ");
                    }
                }

                whereBuilder.Append(")");
            }

            return whereBuilder.ToString();
        }

        private static TimeSpan GetCacheTimeout()
        {
            //Since this cache can be quite large (http://issues.umbraco.org/issue/U4-2161) we will only have this exist in cache for 20 minutes, 
            // then it will refresh from the database.
            return new TimeSpan(0, 20, 0);
        }

        private static CacheItemPriority GetCachePriority()
        {
            //Since this cache can be quite large (http://issues.umbraco.org/issue/U4-2161) we will make this priority below average
            return CacheItemPriority.BelowNormal;
        }

        private static IEnumerable<UserGroupEntityPermission> ConvertToPermissionList(IEnumerable<UserGroup2NodePermissionDto> result)
        {
            var permissions = new List<UserGroupEntityPermission>();
            var nodePermissions = result.GroupBy(x => x.NodeId);
            foreach (var np in nodePermissions)
            {
                var userGroupPermissions = np.GroupBy(x => x.UserGroupId);
                foreach (var permission in userGroupPermissions)
                {
                    var perms = permission.Select(x => x.Permission).ToArray();
                    permissions.Add(new UserGroupEntityPermission(permission.Key, permission.First().NodeId, perms));
                }
            }

            return permissions;
        }

        /// <summary>
        /// Returns permissions for all groups for a given entity
        /// </summary>
        /// <param name="entityId"></param>
        /// <returns></returns>
        public IEnumerable<UserGroupEntityPermission> GetPermissionsForEntity(int entityId)
        {
            var sql = new Sql();
            sql.Select("*")
                .From<UserGroup2NodePermissionDto>()
                .Where<UserGroup2NodePermissionDto>(dto => dto.NodeId == entityId)
                .OrderBy<UserGroup2NodePermissionDto>(dto => dto.NodeId);

            var result = _unitOfWork.Database.Fetch<UserGroup2NodePermissionDto>(sql).ToArray();
                // ToArray() to ensure it's all fetched from the db once
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
        public void ReplacePermissions(int groupId, IEnumerable<char> permissions, params int[] entityIds)
        {
            var db = _unitOfWork.Database;

            //we need to batch these in groups of 2000 so we don't exceed the max 2100 limit
                var sql = "DELETE FROM umbracoUserGroup2NodePermission WHERE userGroupId = @groupId AND nodeId in (@nodeIds)";
            foreach (var idGroup in entityIds.InGroupsOf(2000))
            {
                    db.Execute(sql, new {groupId = groupId, nodeIds = idGroup});
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

            _unitOfWork.Database.BulkInsertRecords(toInsert, _sqlSyntax);
            
            //Raise the event
            //TODO: FIX THIS
            _unitOfWork.Events.Dispatch(AssignedPermissions, this, new SaveEventArgs<EntityPermission>(ConvertToPermissionList(toInsert), false));
             AssignedPermissions.RaiseEvent(
                    new SaveEventArgs<UserGroupEntityPermission>(ConvertToPermissionList(toInsert), false), this)

        }

        /// <summary>
        /// Assigns one permission for a user to many entities
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="permission"></param>
        /// <param name="entityIds"></param>
        public void AssignPermission(int groupId, char permission, params int[] entityIds)
        {
            var db = _unitOfWork.Database;
                var sql = "DELETE FROM umbracoUserGroup2NodePermission WHERE userGroupId = @groupId AND permission=@permission AND nodeId in (@entityIds)";
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

            _unitOfWork.Database.BulkInsertRecords(actions, _sqlSyntax);
            
            //Raise the event
            //TODO: Fix this
            _unitOfWork.Events.Dispatch(AssignedPermissions, this, new SaveEventArgs<EntityPermission>(ConvertToPermissionList(actions), false));
            AssignedPermissions.RaiseEvent(
                    new SaveEventArgs<UserGroupEntityPermission>(ConvertToPermissionList(actions), false), this)

        }

        /// <summary>
        /// Assigns one permission to an entity for multiple groups
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="permission"></param>
        /// <param name="groupIds"></param>
        public void AssignEntityPermission(TEntity entity, char permission, IEnumerable<int> groupIds)
        {
            var db = _unitOfWork.Database;
                var sql = "DELETE FROM umbracoUserGroup2NodePermission WHERE nodeId = @nodeId AND permission = @permission AND userGroupId in (@groupIds)";
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

            _unitOfWork.Database.BulkInsertRecords(actions, _sqlSyntax);

            //Raise the event
            //TODO: Fix this
            _unitOfWork.Events.Dispatch(AssignedPermissions, this, new SaveEventArgs<EntityPermission>(ConvertToPermissionList(actions), false));
             AssignedPermissions.RaiseEvent(
                    new SaveEventArgs<UserGroupEntityPermission>(ConvertToPermissionList(actions), false), this)

        }

        /// <summary>
        /// Assigns permissions to an entity for multiple users/permission entries
        /// </summary>
        /// <param name="permissionSet">
        /// </param>
        /// <remarks>
        /// This will first clear the permissions for this entity then re-create them
        /// </remarks>
        public void ReplaceEntityPermissions(EntityPermissionSet permissionSet)
        {
            var db = _unitOfWork.Database;
                var sql = "DELETE FROM umbracoUserGroup2NodePermission WHERE nodeId = @nodeId";
                db.Execute(sql, new {nodeId = permissionSet.EntityId});

                var actions = permissionSet.PermissionsSet.Select(p => new UserGroup2NodePermissionDto
            {
                NodeId = permissionSet.EntityId,
                Permission = p.Permission,
                    UserGroupId = p.UserGroupId
            }).ToArray();

            _unitOfWork.Database.BulkInsertRecords(actions, _sqlSyntax);
            
            //Raise the event
            //TODO: Fix this
            _unitOfWork.Events.Dispatch(AssignedPermissions, this, new SaveEventArgs<EntityPermission>(ConvertToPermissionList(actions), false));
            AssignedPermissions.RaiseEvent(
                    new SaveEventArgs<UserGroupEntityPermission>(ConvertToPermissionList(actions), false), this)

            }
        }

        public static event TypedEventHandler<PermissionRepository<TEntity>, SaveEventArgs<UserGroupEntityPermission>> AssignedPermissions;
    }
}