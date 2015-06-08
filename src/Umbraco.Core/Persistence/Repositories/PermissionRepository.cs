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

namespace Umbraco.Core.Persistence.Repositories
{
    /// <summary>
    /// A repository that exposes functionality to modify assigned permissions to a node
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    internal class PermissionRepository<TEntity>
        where TEntity : class, IAggregateRoot
    {
        private readonly IDatabaseUnitOfWork _unitOfWork;
        private readonly CacheHelper _cache;
        private readonly ISqlSyntaxProvider _sqlSyntax;

        internal PermissionRepository(IDatabaseUnitOfWork unitOfWork, CacheHelper cache, ISqlSyntaxProvider sqlSyntax)
        {
            _unitOfWork = unitOfWork;
            _cache = cache;
            _sqlSyntax = sqlSyntax;
        }

        /// <summary>
        /// Returns permissions for a given user for any number of nodes
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="entityIds"></param>
        /// <returns></returns>        
        public IEnumerable<EntityPermission> GetUserPermissionsForEntities(int userId, params int[] entityIds)
        {
            var entityIdKey = string.Join(",", entityIds.Select(x => x.ToString(CultureInfo.InvariantCulture)));
            return _cache.RuntimeCache.GetCacheItem<IEnumerable<EntityPermission>>(
                string.Format("{0}{1}{2}", CacheKeys.UserPermissionsCacheKey, userId, entityIdKey),
                () =>
        {            

                    var whereBuilder = new StringBuilder();
            
                    //where userId = @userId AND
                    whereBuilder.Append(_sqlSyntax.GetQuotedColumnName("userId"));
                    whereBuilder.Append("=");
                    whereBuilder.Append(userId);

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

                    var sql = new Sql();
                    sql.Select("*")
                        .From<User2NodePermissionDto>()
                        .Where(whereBuilder.ToString());

                    //ToArray() to ensure it's all fetched from the db once.
                    var result = _unitOfWork.Database.Fetch<User2NodePermissionDto>(sql).ToArray();
                    return ConvertToPermissionList(result);

                },
                //Since this cache can be quite large (http://issues.umbraco.org/issue/U4-2161) we will only have this exist in cache for 20 minutes, 
                // then it will refresh from the database.
                new TimeSpan(0, 20, 0),
                //Since this cache can be quite large (http://issues.umbraco.org/issue/U4-2161) we will make this priority below average
                priority: CacheItemPriority.BelowNormal);

        } 

        /// <summary>
        /// Returns permissions for all users for a given entity
        /// </summary>
        /// <param name="entityId"></param>
        /// <returns></returns>
        public IEnumerable<EntityPermission> GetPermissionsForEntity(int entityId)
        {
            var sql = new Sql();
            sql.Select("*")
               .From<User2NodePermissionDto>()
               .Where<User2NodePermissionDto>(dto => dto.NodeId == entityId)
               .OrderBy<User2NodePermissionDto>(dto => dto.NodeId);

            //ToArray() to ensure it's all fetched from the db once.
            var result = _unitOfWork.Database.Fetch<User2NodePermissionDto>(sql).ToArray();
            return ConvertToPermissionList(result);
        }

        /// <summary>
        /// Assigns the same permission set for a single user to any number of entities
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="permissions"></param>
        /// <param name="entityIds"></param>
        /// <remarks>
        /// This will first clear the permissions for this user and entities and recreate them
        /// </remarks>
        public void ReplaceUserPermissions(int userId, IEnumerable<char> permissions, params int[] entityIds)
        {
            var db = _unitOfWork.Database;
            using (var trans = db.GetTransaction())
            {
                db.Execute("DELETE FROM umbracoUser2NodePermission WHERE userId=@userId AND nodeId in (@nodeIds)",
                    new {userId = userId, nodeIds = entityIds});

                var toInsert = new List<User2NodePermissionDto>();
                foreach (var p in permissions)
                {
                    foreach (var e in entityIds)
                    {
                        toInsert.Add(new User2NodePermissionDto
                        {
                            NodeId = e,
                            Permission = p.ToString(CultureInfo.InvariantCulture),
                            UserId = userId
                        });
                    }
                }

                _unitOfWork.Database.BulkInsertRecords(toInsert, trans);

                trans.Complete();

                //Raise the event
                AssignedPermissions.RaiseEvent(
                    new SaveEventArgs<EntityPermission>(ConvertToPermissionList(toInsert), false), this);
            }
        }

        /// <summary>
        /// Assigns one permission for a user to many entities
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="permission"></param>
        /// <param name="entityIds"></param>
        public void AssignUserPermission(int userId, char permission, params int[] entityIds)
        {
            var db = _unitOfWork.Database;
            using (var trans = db.GetTransaction())
            {
                db.Execute("DELETE FROM umbracoUser2NodePermission WHERE userId=@userId AND permission=@permission AND nodeId in (@entityIds)",
                    new
                    {
                        userId = userId,
                        permission = permission.ToString(CultureInfo.InvariantCulture),
                        entityIds = entityIds
                    });

                var actions = entityIds.Select(id => new User2NodePermissionDto
                {
                    NodeId = id,
                    Permission = permission.ToString(CultureInfo.InvariantCulture),
                    UserId = userId
                }).ToArray();

                _unitOfWork.Database.BulkInsertRecords(actions, trans);

                trans.Complete();

                //Raise the event
                AssignedPermissions.RaiseEvent(
                    new SaveEventArgs<EntityPermission>(ConvertToPermissionList(actions), false), this);
            }
        } 

        /// <summary>
        /// Assigns one permission to an entity for multiple users
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="permission"></param>
        /// <param name="userIds"></param>
        public void AssignEntityPermission(TEntity entity, char permission, IEnumerable<int> userIds)
        {
            var db = _unitOfWork.Database;
            using (var trans = db.GetTransaction())
            {
                db.Execute("DELETE FROM umbracoUser2NodePermission WHERE nodeId=@nodeId AND permission=@permission AND userId in (@userIds)",
                    new
                    {
                        nodeId = entity.Id, 
                        permission = permission.ToString(CultureInfo.InvariantCulture),
                        userIds = userIds
                    });

                var actions = userIds.Select(id => new User2NodePermissionDto
                {
                    NodeId = entity.Id,
                    Permission = permission.ToString(CultureInfo.InvariantCulture),
                    UserId = id
                }).ToArray();

                _unitOfWork.Database.BulkInsertRecords(actions, trans);

                trans.Complete();

                //Raise the event
                AssignedPermissions.RaiseEvent(
                    new SaveEventArgs<EntityPermission>(ConvertToPermissionList(actions), false), this);
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
        public void ReplaceEntityPermissions(EntityPermissionSet permissionSet)
        {
            var db = _unitOfWork.Database;
            using (var trans = db.GetTransaction())
            {
                db.Execute("DELETE FROM umbracoUser2NodePermission WHERE nodeId=@nodeId", new { nodeId = permissionSet.EntityId });

                var actions = permissionSet.UserPermissionsSet.Select(p => new User2NodePermissionDto
                {
                    NodeId = permissionSet.EntityId,
                    Permission = p.Permission,
                    UserId = p.UserId
                }).ToArray();

                _unitOfWork.Database.BulkInsertRecords(actions, trans);

                trans.Complete();

                //Raise the event
                AssignedPermissions.RaiseEvent(
                    new SaveEventArgs<EntityPermission>(ConvertToPermissionList(actions), false), this);
            }            
        }

        private static IEnumerable<EntityPermission> ConvertToPermissionList(IEnumerable<User2NodePermissionDto> result)
        {
            var permissions = new List<EntityPermission>();
            var nodePermissions = result.GroupBy(x => x.NodeId);
            foreach (var np in nodePermissions)
            {
                var userPermissions = np.GroupBy(x => x.UserId);
                foreach (var up in userPermissions)
                {
                    var perms = up.Select(x => x.Permission).ToArray();
                    permissions.Add(new EntityPermission(up.Key, up.First().NodeId, perms));
                }
            }
            return permissions;
        }

        public static event TypedEventHandler<PermissionRepository<TEntity>, SaveEventArgs<EntityPermission>> AssignedPermissions;
    }
}