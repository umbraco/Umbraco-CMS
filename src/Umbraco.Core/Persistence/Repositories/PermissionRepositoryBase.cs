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
    using System;
    using System.Web.Caching;

    /// <summary>
    /// A repository that exposes functionality to modify assigned permissions to a node
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    internal abstract class PermissionRepositoryBase<TEntity>
        where TEntity : class, IAggregateRoot
    {
        protected PermissionRepositoryBase(IDatabaseUnitOfWork unitOfWork, ISqlSyntaxProvider sqlSyntax)
        {
            UnitOfWork = unitOfWork;
            SqlSyntax = sqlSyntax;
        }

        protected IDatabaseUnitOfWork UnitOfWork { get; private set; }

        protected ISqlSyntaxProvider SqlSyntax { get; private set; }

        protected static string GetEntityIdKey(int[] entityIds)
        {
            return String.Join(",", entityIds.Select(x => x.ToString(CultureInfo.InvariantCulture)));
        }

        protected string GetPermissionsForEntitiesCriteria(int userOrGroupId, bool forUser, params int[] entityIds)
        {
            var whereBuilder = new StringBuilder();
            whereBuilder.Append(SqlSyntax.GetQuotedColumnName(forUser ? "userId" : "userGroupId"));
            whereBuilder.Append("=");
            whereBuilder.Append(userOrGroupId);

            if (entityIds.Any())
            {
                whereBuilder.Append(" AND ");

                //where nodeId = @nodeId1 OR nodeId = @nodeId2, etc...
                whereBuilder.Append("(");
                for (var index = 0; index < entityIds.Length; index++)
                {
                    var entityId = entityIds[index];
                    whereBuilder.Append(SqlSyntax.GetQuotedColumnName("nodeId"));
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

        protected static TimeSpan GetCacheTimeout()
        {
            //Since this cache can be quite large (http://issues.umbraco.org/issue/U4-2161) we will only have this exist in cache for 20 minutes, 
            // then it will refresh from the database.
            return new TimeSpan(0, 20, 0);
        }

        protected static CacheItemPriority GetCachePriority()
        {
            //Since this cache can be quite large (http://issues.umbraco.org/issue/U4-2161) we will make this priority below average
            return CacheItemPriority.BelowNormal;
        }

        protected static IEnumerable<UserEntityPermission> ConvertToPermissionList(IEnumerable<User2NodePermissionDto> result)
        {
            var permissions = new List<UserEntityPermission>();
            var nodePermissions = result.GroupBy(x => x.NodeId);
            foreach (var np in nodePermissions)
            {
                var userPermissions = np.GroupBy(x => x.UserId);
                foreach (var up in userPermissions)
                {
                    var perms = up.Select(x => x.Permission).ToArray();
                    permissions.Add(new UserEntityPermission(up.Key, up.First().NodeId, perms));
                }
            }

            return permissions;
        }

        protected static IEnumerable<GroupEntityPermission> ConvertToPermissionList(IEnumerable<UserGroup2NodePermissionDto> result)
        {
            var permissions = new List<GroupEntityPermission>();
            var nodePermissions = result.GroupBy(x => x.NodeId);
            foreach (var np in nodePermissions)
            {
                var userGroupPermissions = np.GroupBy(x => x.UserGroupId);
                foreach (var gp in userGroupPermissions)
                {
                    var perms = gp.Select(x => x.Permission).ToArray();
                    permissions.Add(new GroupEntityPermission(gp.Key, gp.First().NodeId, perms));
                }
            }

            return permissions;
        }

        protected abstract string GetTableName();

        protected abstract string GetFieldName();
    }
}