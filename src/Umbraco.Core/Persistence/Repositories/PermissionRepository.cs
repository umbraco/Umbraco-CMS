using System;
using System.Collections.Generic;
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
using CacheKeys = Umbraco.Core.Cache.CacheKeys;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Querying;

namespace Umbraco.Core.Persistence.Repositories
{
    /// <summary>
    /// A repository that exposes functionality to modify assigned permissions to a node
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <remarks>
    /// This repo implements the base <see cref="PetaPocoRepositoryBase{TId,TEntity}"/> class so that permissions can be queued to be persisted
    /// like the normal repository pattern but the standard repository Get commands don't apply and will throw <see cref="NotImplementedException"/>
    /// </remarks>
    internal class PermissionRepository<TEntity> : PetaPocoRepositoryBase<int, ContentPermissionSet>
        where TEntity : class, IAggregateRoot
    {

        public PermissionRepository(IScopeUnitOfWork work, CacheHelper cache, ILogger logger, ISqlSyntaxProvider sqlSyntax)
            : base(work, cache, logger, sqlSyntax)
        {
            
        }

        /// <summary>
        /// Returns explicitly defined permissions for a user group for any number of nodes
        /// </summary>
        /// <param name="groupIds">
        /// The group ids to lookup permissions for
        /// </param>
        /// <param name="entityIds"></param>
        /// <returns></returns>        
        /// <remarks>
        /// This method will not support passing in more than 2000 group Ids
        /// </remarks>
        public EntityPermissionCollection GetPermissionsForEntities(int[] groupIds, params int[] entityIds)
        {
            var result = new EntityPermissionCollection();

            foreach (var groupOfGroupIds in groupIds.InGroupsOf(2000))
            {
                //copy local
                var localIds = groupOfGroupIds.ToArray();

                if (entityIds.Length == 0)
                {
                    var sql = new Sql();                    
                    sql.Select("*")
                        .From<UserGroup2NodePermissionDto>(SqlSyntax)
                        .Where<UserGroup2NodePermissionDto>(dto => localIds.Contains(dto.UserGroupId), SqlSyntax);
                    var permissions = UnitOfWork.Database.Fetch<UserGroup2NodePermissionDto>(sql);
                    foreach (var permission in ConvertToPermissionList(permissions))
                    {
                        result.Add(permission);
                    }
                }
                else
                {
                    //iterate in groups of 2000 since we don't want to exceed the max SQL param count
                    foreach (var groupOfEntityIds in entityIds.InGroupsOf(2000))
                    {
                        var ids = groupOfEntityIds;
                        var sql = new Sql();
                        sql.Select("*")
                            .From<UserGroup2NodePermissionDto>(SqlSyntax)
                            .Where<UserGroup2NodePermissionDto>(dto => localIds.Contains(dto.UserGroupId) && ids.Contains(dto.NodeId), SqlSyntax);
                        var permissions = UnitOfWork.Database.Fetch<UserGroup2NodePermissionDto>(sql);
                        foreach (var permission in ConvertToPermissionList(permissions))
                        {
                            result.Add(permission);
                        }
                    }
                }
            }
            
            return result;
        }        

        /// <summary>
        /// Returns permissions directly assigned to the content items for all user groups
        /// </summary>
        /// <param name="entityIds"></param>
        /// <returns></returns>
        public IEnumerable<EntityPermission> GetPermissionsForEntities(int[] entityIds)
        {
            var sql = new Sql();
            sql.Select("*")
                .From<UserGroup2NodePermissionDto>(SqlSyntax)
                .Where<UserGroup2NodePermissionDto>(dto => entityIds.Contains(dto.NodeId), SqlSyntax)
                .OrderBy<UserGroup2NodePermissionDto>(dto => dto.NodeId, SqlSyntax);

            var result = UnitOfWork.Database.Fetch<UserGroup2NodePermissionDto>(sql);
            return ConvertToPermissionList(result);
        }

        /// <summary>
        /// Returns permissions directly assigned to the content item for all user groups
        /// </summary>
        /// <param name="entityId"></param>
        /// <returns></returns>
        public EntityPermissionCollection GetPermissionsForEntity(int entityId)
        {
            var sql = new Sql();
            sql.Select("*")
                .From<UserGroup2NodePermissionDto>(SqlSyntax)
                .Where<UserGroup2NodePermissionDto>(dto => dto.NodeId == entityId, SqlSyntax)
                .OrderBy<UserGroup2NodePermissionDto>(dto => dto.NodeId, SqlSyntax);

            var result = UnitOfWork.Database.Fetch<UserGroup2NodePermissionDto>(sql);
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
            if (entityIds.Length == 0)
                return;

            var db = UnitOfWork.Database;

            //we need to batch these in groups of 2000 so we don't exceed the max 2100 limit
            var sql = "DELETE FROM umbracoUserGroup2NodePermission WHERE userGroupId = @groupId AND nodeId in (@nodeIds)";
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

            UnitOfWork.Database.BulkInsertRecords(toInsert, SqlSyntax);
            
        }

        /// <summary>
        /// Assigns one permission for a user to many entities
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="permission"></param>
        /// <param name="entityIds"></param>
        public void AssignPermission(int groupId, char permission, params int[] entityIds)
        {
            var db = UnitOfWork.Database;
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

            UnitOfWork.Database.BulkInsertRecords(actions, SqlSyntax);
            
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

            UnitOfWork.Database.BulkInsertRecords(actions, SqlSyntax);
            
        }

        /// <summary>
        /// Assigns permissions to an entity for multiple group/permission entries
        /// </summary>
        /// <param name="permissionSet">
        /// </param>
        /// <remarks>
        /// This will first clear the permissions for this entity then re-create them
        /// </remarks>
        public void ReplaceEntityPermissions(EntityPermissionSet permissionSet)
        {
            var db = UnitOfWork.Database;
            var sql = "DELETE FROM umbracoUserGroup2NodePermission WHERE nodeId = @nodeId";
            db.Execute(sql, new { nodeId = permissionSet.EntityId });

            var toInsert = new List<UserGroup2NodePermissionDto>();
            foreach (var entityPermission in permissionSet.PermissionsSet)
            {
                foreach (var permission in entityPermission.AssignedPermissions)
                {
                    toInsert.Add(new UserGroup2NodePermissionDto
                    {
                        NodeId = permissionSet.EntityId,
                        Permission = permission,
                        UserGroupId = entityPermission.UserGroupId
                    });
                }
            }

            UnitOfWork.Database.BulkInsertRecords(toInsert, SqlSyntax);
            
        }


        #region Not implemented (don't need to for the purposes of this repo)
        protected override ContentPermissionSet PerformGet(int id)
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<ContentPermissionSet> PerformGetAll(params int[] ids)
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<ContentPermissionSet> PerformGetByQuery(IQuery<ContentPermissionSet> query)
        {
            throw new NotImplementedException();
        }

        protected override Sql GetBaseQuery(bool isCount)
        {
            throw new NotImplementedException();
        }

        protected override string GetBaseWhereClause()
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<string> GetDeleteClauses()
        {
            return new List<string>();
        }

        protected override Guid NodeObjectTypeId
        {
            get { throw new NotImplementedException(); }
        }
        
        protected override void PersistDeletedItem(ContentPermissionSet entity)
        {
            throw new NotImplementedException();
        }

        #endregion

        /// <summary>
        /// Used to add or update entity permissions during a content item being updated
        /// </summary>
        /// <param name="entity"></param>
        protected override void PersistNewItem(ContentPermissionSet entity)
        {
            //does the same thing as update
            PersistUpdatedItem(entity);
        }

        /// <summary>
        /// Used to add or update entity permissions during a content item being updated
        /// </summary>
        /// <param name="entity"></param>
        protected override void PersistUpdatedItem(ContentPermissionSet entity)
        {
            var asAggregateRoot = (IAggregateRoot)entity;
            if (asAggregateRoot.HasIdentity == false)
            {
                throw new InvalidOperationException("Cannot create permissions for an entity without an Id");
            }

            ReplaceEntityPermissions(entity);
        }

        private static EntityPermissionCollection ConvertToPermissionList(IEnumerable<UserGroup2NodePermissionDto> result)
        {
            var permissions = new EntityPermissionCollection();
            var nodePermissions = result.GroupBy(x => x.NodeId);
            foreach (var np in nodePermissions)
            {
                var userGroupPermissions = np.GroupBy(x => x.UserGroupId);
                foreach (var permission in userGroupPermissions)
                {
                    var perms = permission.Select(x => x.Permission).Distinct().ToArray();
                    permissions.Add(new EntityPermission(permission.Key, np.Key, perms));
                }
            }

            return permissions;
        }
        
    }
}