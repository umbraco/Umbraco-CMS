using System;
using System.Collections.Generic;
using System.Dynamic;
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
    /// <typeparam name="TId"></typeparam>
    /// <typeparam name="TEntity"></typeparam>
    internal abstract class PermissionRepository<TId, TEntity> : PetaPocoRepositoryBase<TId, TEntity>
        where TEntity : class, IAggregateRoot
    {
        protected PermissionRepository(IDatabaseUnitOfWork work)
            : base(work)
        {
        }

        protected PermissionRepository(IDatabaseUnitOfWork work, IRepositoryCacheProvider cache)
            : base(work, cache)
        {
        }

        protected internal IEnumerable<User2NodePermissionDto> GetPermissionsForEntity(int entityId)
        {
            var sql = new Sql();
            sql.Select("*")
                .From<User2NodePermissionDto>()
                .Where<User2NodePermissionDto>(dto => dto.NodeId == entityId);
            return Database.Fetch<User2NodePermissionDto>(sql);
        }

        /// <summary>
        /// Assigns permissions to an entity for multiple users
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="permissions">
        /// A list of permissions to assign - currently in Umbraco each permission is a single char but this list of strings allows for flexibility in the future
        /// </param>
        /// <param name="userIds"></param>
        protected internal void AssignEntityPermissions(TEntity entity, IEnumerable<string> permissions, IEnumerable<object> userIds)
        {
            var actions = userIds.Select(id => new User2NodePermissionDto
                {
                    NodeId = entity.Id,
                    Permission = string.Join("",permissions),
                    UserId = (int)id
                });

            Database.BulkInsertRecords(actions);
        }

        /// <summary>
        /// Assigns permissions to an entity for multiple users/permission entries
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="userPermissions">
        /// A key/value pair list containing a userId and a permission to assign, currently in Umbraco each permission is a single char but this list of strings allows for flexibility in the future
        /// </param>
        protected internal void AssignEntityPermissions(TEntity entity, IEnumerable<KeyValuePair<object, IEnumerable<string>>> userPermissions)
        {
            var actions = userPermissions.Select(p => new User2NodePermissionDto
            {
                NodeId = entity.Id,
                Permission = string.Join("", p.Value),
                UserId = (int)p.Key
            });

            Database.BulkInsertRecords(actions);
        }

        /// <summary>
        /// Replace permissions for an entity for multiple users
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="permissions">
        /// A list of permissions to assign - currently in Umbraco each permission is a single char but this list of strings allows for flexibility in the future
        /// </param>
        /// <param name="userIds"></param>
        protected internal void ReplaceEntityPermissions(TEntity entity, IEnumerable<string> permissions, IEnumerable<object> userIds)
        {            
            Database.Update<User2NodePermissionDto>(
                GenerateReplaceEntityPermissionsSql(entity.Id, permissions, userIds.ToArray()));
        }

        /// <summary>
        /// An overload to replace entity permissions and all replace all descendant permissions
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="permissions">
        /// A list of permissions to assign - currently in Umbraco each permission is a single char but this list of strings allows for flexibility in the future
        /// </param>
        /// <param name="getDescendantIds">
        /// A callback to get the descendant Ids of the current entity
        /// </param>
        /// <param name="userIds"></param>
        protected internal void ReplaceEntityPermissions(TEntity entity, IEnumerable<string> permissions, Func<IEntity, IEnumerable<int>> getDescendantIds, IEnumerable<object> userIds)
        {
            Database.Update<User2NodePermissionDto>(
                GenerateReplaceEntityPermissionsSql(
                new[] {entity.Id}.Concat(getDescendantIds(entity)).ToArray(), 
                permissions, 
                userIds.ToArray()));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="permissions">
        /// A list of permissions to assign - currently in Umbraco each permission is a single char but this list of strings allows for flexibility in the future
        /// </param>
        /// <param name="userIds"></param>
        /// <returns></returns>
        internal static string GenerateReplaceEntityPermissionsSql(int entityId, IEnumerable<string> permissions, object[] userIds)
        {
            return GenerateReplaceEntityPermissionsSql(new[] {entityId}, permissions, userIds);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entityIds"></param>
        /// <param name="permissions">
        /// A list of permissions to assign - currently in Umbraco each permission is a single char but this list of strings allows for flexibility in the future
        /// </param>
        /// <param name="userIds"></param>
        /// <returns></returns>
        internal static string GenerateReplaceEntityPermissionsSql(int[] entityIds, IEnumerable<string> permissions, object[] userIds)
        {
            //create the "SET" clause of the update statement
            var sqlSet = string.Format("SET {0}={1}",
                                    SqlSyntaxContext.SqlSyntaxProvider.GetQuotedColumnName("permission"),
                                    SqlSyntaxContext.SqlSyntaxProvider.GetQuotedValue(string.Join("", permissions)));

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