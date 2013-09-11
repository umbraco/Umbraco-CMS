using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Text;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.Caching;
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
        private readonly IDatabaseUnitOfWork _unitOfWork;

        internal PermissionRepository(IDatabaseUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Returns permissions for a given user for any number of nodes
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="entityIds"></param>
        /// <returns></returns>        
        internal IEnumerable<EntityPermission> GetUserPermissionsForEntities(object userId, params int[] entityIds)
        {            
            var whereBuilder = new StringBuilder();
            
            //where userId = @userId AND
            whereBuilder.Append(SqlSyntaxContext.SqlSyntaxProvider.GetQuotedColumnName("userId"));
            whereBuilder.Append("=");
            whereBuilder.Append(userId);
            whereBuilder.Append(" AND ");

            //where nodeId = @nodeId1 OR nodeId = @nodeId2, etc...
            whereBuilder.Append("(");
            for (var index = 0; index < entityIds.Length; index++)
            {
                var entityId = entityIds[index];
                whereBuilder.Append(SqlSyntaxContext.SqlSyntaxProvider.GetQuotedColumnName("nodeId"));
                whereBuilder.Append("=");
                whereBuilder.Append(entityId);
                if (index < entityIds.Length - 1)
                {
                    whereBuilder.Append(" OR ");
                }
            }
            whereBuilder.Append(")");

            var sql = new Sql();
            sql.Select("*")
                .From<User2NodePermissionDto>()
                .Where(whereBuilder.ToString());

            //ToArray() to ensure it's all fetched from the db once.
            var result = _unitOfWork.Database.Fetch<User2NodePermissionDto>(sql).ToArray();
            return ConvertToPermissionList(result);
        } 

        /// <summary>
        /// Returns permissions for all users for a given entity
        /// </summary>
        /// <param name="entityId"></param>
        /// <returns></returns>
        internal IEnumerable<EntityPermission> GetPermissionsForEntity(int entityId)
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

        private IEnumerable<EntityPermission> ConvertToPermissionList(IEnumerable<User2NodePermissionDto> result)
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

        /// <summary>
        /// Assigns one permission to an entity for multiple users
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="permission"></param>
        /// <param name="userIds"></param>
        internal void AssignEntityPermissions(TEntity entity, char permission, IEnumerable<object> userIds)
        {
            var actions = userIds.Select(id => new User2NodePermissionDto
                {
                    NodeId = entity.Id,
                    Permission = permission.ToString(CultureInfo.InvariantCulture),
                    UserId = (int)id
                });

            _unitOfWork.Database.BulkInsertRecords(actions);
        }

        /// <summary>
        /// Assigns permissions to an entity for multiple users/permission entries
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="userPermissions">
        /// A key/value pair list containing a userId and a permission to assign
        /// </param>
        internal void AssignEntityPermissions(TEntity entity, IEnumerable<Tuple<object, string>> userPermissions)
        {
            var actions = userPermissions.Select(p => new User2NodePermissionDto
            {
                NodeId = entity.Id,
                Permission = p.Item2,
                UserId = (int)p.Item1
            });

            _unitOfWork.Database.BulkInsertRecords(actions);
        }

        /// <summary>
        /// Replace permissions for an entity for multiple users
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="permissions"></param>
        /// <param name="userIds"></param>
        internal void ReplaceEntityPermissions(TEntity entity, string permissions, IEnumerable<object> userIds)
        {
            _unitOfWork.Database.Update<User2NodePermissionDto>(
                GenerateReplaceEntityPermissionsSql(entity.Id, permissions, userIds.ToArray()));
        }

        /// <summary>
        /// An overload to replace entity permissions and all replace all descendant permissions
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="permissions"></param>
        /// <param name="getDescendantIds">
        /// A callback to get the descendant Ids of the current entity
        /// </param>
        /// <param name="userIds"></param>
        internal void ReplaceEntityPermissions(TEntity entity, string permissions, Func<IEntity, IEnumerable<int>> getDescendantIds, IEnumerable<object> userIds)
        {
            _unitOfWork.Database.Update<User2NodePermissionDto>(
                GenerateReplaceEntityPermissionsSql(
                new[] {entity.Id}.Concat(getDescendantIds(entity)).ToArray(), 
                permissions, 
                userIds.ToArray()));
        }

        internal static string GenerateReplaceEntityPermissionsSql(int entityId, string permissions, object[] userIds)
        {
            return GenerateReplaceEntityPermissionsSql(new[] {entityId}, permissions, userIds);
        }

        internal static string GenerateReplaceEntityPermissionsSql(int[] entityIds, string permissions, object[] userIds)
        {
            //create the "SET" clause of the update statement
            var sqlSet = string.Format("SET {0}={1}",
                                    SqlSyntaxContext.SqlSyntaxProvider.GetQuotedColumnName("permission"),
                                    SqlSyntaxContext.SqlSyntaxProvider.GetQuotedValue(permissions));

            //build the nodeIds part of the where clause
            var sqlNodeWhere = BuildOrClause(entityIds, "nodeId");
            
            //build up the userIds part of the where clause
            var userWhereBuilder = BuildOrClause(userIds, "userId");

            var sqlWhere = new Sql();
            sqlWhere.Where(string.Format("{0} AND {1}", sqlNodeWhere, userWhereBuilder));

            return string.Format("{0} {1}", sqlSet, sqlWhere.SQL);
        }

        private static string BuildOrClause<T>(IEnumerable<T> ids, string colName)
        {
            var asArray = ids.ToArray();
            var userWhereBuilder = new StringBuilder();
            userWhereBuilder.Append("(");
            for (var index = 0; index < asArray.Length; index++)
            {
                var userId = asArray[index];
                userWhereBuilder.Append(SqlSyntaxContext.SqlSyntaxProvider.GetQuotedColumnName(colName));
                userWhereBuilder.Append("=");
                userWhereBuilder.Append(userId);
                if (index < asArray.Length - 1)
                {
                    userWhereBuilder.Append(" OR ");
                }
            }
            userWhereBuilder.Append(")");
            return userWhereBuilder.ToString();
        }
    }
}