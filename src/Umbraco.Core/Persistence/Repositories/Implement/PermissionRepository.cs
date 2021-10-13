using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NPoco;
using Umbraco.Core.Cache;
using Umbraco.Core.Exceptions;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Persistence.Repositories.Implement
{
    /// <summary>
    /// A (sub) repository that exposes functionality to modify assigned permissions to a node
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <remarks>
    /// This repo implements the base <see cref="NPocoRepositoryBase{TId,TEntity}"/> class so that permissions can be queued to be persisted
    /// like the normal repository pattern but the standard repository Get commands don't apply and will throw <see cref="NotImplementedException"/>
    /// </remarks>
    internal class PermissionRepository<TEntity> : NPocoRepositoryBase<int, ContentPermissionSet>
        where TEntity : class, IEntity
    {
        public PermissionRepository(IScopeAccessor scopeAccessor, AppCaches cache, ILogger logger)
            : base(scopeAccessor, cache, logger)
        { }

        /// <summary>
        /// Returns explicitly defined permissions for a user group for any number of nodes
        /// </summary>
        /// <param name="groupIds">
        /// The group ids to lookup permissions for
        /// </param>
        /// <param name="entityIds"></param>
        /// <returns></returns>
        /// <remarks>
        /// This method will not support passing in more than 2000 group IDs when also passing in entity IDs.
        /// </remarks>
        public EntityPermissionCollection GetPermissionsForEntities(int[] groupIds, params int[] entityIds)
        {
            var result = new EntityPermissionCollection();

            if (entityIds.Length == 0)
            {
                foreach (var group in groupIds.InGroupsOf(Constants.Sql.MaxParameterCount))
                {
                    var sql = Sql()
                        .SelectAll()
                        .From<UserGroup2NodePermissionDto>()
                        .Where<UserGroup2NodePermissionDto>(dto => group.Contains(dto.UserGroupId));

                    var permissions = AmbientScope.Database.Fetch<UserGroup2NodePermissionDto>(sql);
                    foreach (var permission in ConvertToPermissionList(permissions))
                    {
                        result.Add(permission);
                    }
                }
            }
            else
            {
                foreach (var group in entityIds.InGroupsOf(Constants.Sql.MaxParameterCount - groupIds.Length))
                {
                    var sql = Sql()
                        .SelectAll()
                        .From<UserGroup2NodePermissionDto>()
                        .Where<UserGroup2NodePermissionDto>(dto => groupIds.Contains(dto.UserGroupId) && group.Contains(dto.NodeId));

                    var permissions = AmbientScope.Database.Fetch<UserGroup2NodePermissionDto>(sql);
                    foreach (var permission in ConvertToPermissionList(permissions))
                    {
                        result.Add(permission);
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
            var sql = Sql()
                .SelectAll()
                .From<UserGroup2NodePermissionDto>()
                .Where<UserGroup2NodePermissionDto>(dto => entityIds.Contains(dto.NodeId))
                .OrderBy<UserGroup2NodePermissionDto>(dto => dto.NodeId);

            var result = AmbientScope.Database.Fetch<UserGroup2NodePermissionDto>(sql);
            return ConvertToPermissionList(result);
        }

        /// <summary>
        /// Returns permissions directly assigned to the content item for all user groups
        /// </summary>
        /// <param name="entityId"></param>
        /// <returns></returns>
        public EntityPermissionCollection GetPermissionsForEntity(int entityId)
        {
            var sql = Sql()
                .SelectAll()
                .From<UserGroup2NodePermissionDto>()
                .Where<UserGroup2NodePermissionDto>(dto => dto.NodeId == entityId)
                .OrderBy<UserGroup2NodePermissionDto>(dto => dto.NodeId);

            var result = AmbientScope.Database.Fetch<UserGroup2NodePermissionDto>(sql);
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

            var db = AmbientScope.Database;

            var sql = "DELETE FROM umbracoUserGroup2NodePermission WHERE userGroupId = @groupId AND nodeId in (@nodeIds)";
            foreach (var group in entityIds.InGroupsOf(Constants.Sql.MaxParameterCount))
            {
                db.Execute(sql, new { groupId, nodeIds = group });
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

            db.BulkInsertRecords(toInsert);
        }

        /// <summary>
        /// Assigns one permission for a user to many entities
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="permission"></param>
        /// <param name="entityIds"></param>
        public void AssignPermission(int groupId, char permission, params int[] entityIds)
        {
            var db = AmbientScope.Database;

            var sql = "DELETE FROM umbracoUserGroup2NodePermission WHERE userGroupId = @groupId AND permission=@permission AND nodeId in (@entityIds)";
            db.Execute(sql,
            new
            {
                groupId,
                permission = permission.ToString(CultureInfo.InvariantCulture),
                entityIds
            });

            var actions = entityIds.Select(id => new UserGroup2NodePermissionDto
            {
                NodeId = id,
                Permission = permission.ToString(CultureInfo.InvariantCulture),
                UserGroupId = groupId
            }).ToArray();

            db.BulkInsertRecords(actions);
        }

        /// <summary>
        /// Assigns one permission to an entity for multiple groups
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="permission"></param>
        /// <param name="groupIds"></param>
        public void AssignEntityPermission(TEntity entity, char permission, IEnumerable<int> groupIds)
        {
            var db = AmbientScope.Database;
            var groupIdsA = groupIds.ToArray();

            const string sql = "DELETE FROM umbracoUserGroup2NodePermission WHERE nodeId = @nodeId AND permission = @permission AND userGroupId in (@groupIds)";
            db.Execute(sql,
                new
                {
                    nodeId = entity.Id,
                    permission = permission.ToString(CultureInfo.InvariantCulture),
                    groupIds = groupIdsA
                });

            var actions = groupIdsA.Select(id => new UserGroup2NodePermissionDto
            {
                NodeId = entity.Id,
                Permission = permission.ToString(CultureInfo.InvariantCulture),
                UserGroupId = id
            }).ToArray();

            db.BulkInsertRecords(actions);
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
            var db = AmbientScope.Database;

            const string sql = "DELETE FROM umbracoUserGroup2NodePermission WHERE nodeId = @nodeId";
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

            db.BulkInsertRecords(toInsert);
        }

        #region Not implemented (don't need to for the purposes of this repo)

        protected override ContentPermissionSet PerformGet(int id)
        {
            throw new InvalidOperationException("This method won't be implemented.");
        }

        protected override IEnumerable<ContentPermissionSet> PerformGetAll(params int[] ids)
        {
            throw new InvalidOperationException("This method won't be implemented.");
        }

        protected override IEnumerable<ContentPermissionSet> PerformGetByQuery(IQuery<ContentPermissionSet> query)
        {
            throw new InvalidOperationException("This method won't be implemented.");
        }

        protected override Sql<ISqlContext> GetBaseQuery(bool isCount)
        {
            throw new InvalidOperationException("This method won't be implemented.");
        }

        protected override string GetBaseWhereClause()
        {
            throw new InvalidOperationException("This method won't be implemented.");
        }

        protected override IEnumerable<string> GetDeleteClauses()
        {
            return new List<string>();
        }

        protected override Guid NodeObjectTypeId => throw new InvalidOperationException("This property won't be implemented.");

        protected override void PersistDeletedItem(ContentPermissionSet entity)
        {
            throw new InvalidOperationException("This method won't be implemented.");
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
            var asIEntity = (IEntity) entity;
            if (asIEntity.HasIdentity == false)
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
