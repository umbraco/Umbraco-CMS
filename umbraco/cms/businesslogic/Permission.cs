using System;
using System.Collections;
using System.Collections.Specialized;
using System.Data;
using System.Runtime.CompilerServices;

using umbraco.DataLayer;
using umbraco.cms.businesslogic;
using System.Collections.Generic;

namespace umbraco.BusinessLogic
{
	/// <summary>
	/// Summary description for Permission.
	/// </summary>
	public class Permission
	{

        public int NodeId { get; private set; }
        public int UserId { get; private set; }
        public char PermissionId { get; private set; }

        /// <summary>
        /// Private constructor, this class cannot be directly instantiated
        /// </summary>
        private Permission() { }

        private static ISqlHelper SqlHelper
        {
            get { return Application.SqlHelper; }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
		public static void MakeNew(BusinessLogic.User User, cms.businesslogic.CMSNode Node, char PermissionKey) 
		{
            IParameter[] parameters = new IParameter[] { SqlHelper.CreateParameter("@userId", User.Id),
                                                         SqlHelper.CreateParameter("@nodeId", Node.Id),
                                                         SqlHelper.CreateParameter("@permission", PermissionKey.ToString()) };

            // Method is synchronized so exists remains consistent (avoiding race condition)
            bool exists = SqlHelper.ExecuteScalar<int>("SELECT COUNT(userId) FROM umbracoUser2nodePermission WHERE userId = @userId AND nodeId = @nodeId AND permission = @permission",
                                                       parameters) > 0;
            if (!exists)
                SqlHelper.ExecuteNonQuery("INSERT INTO umbracoUser2nodePermission (userId, nodeId, permission) VALUES (@userId, @nodeId, @permission)",
                                          parameters);
		}

        /// <summary>
        /// Returns the permissions for a user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static IEnumerable<Permission> GetUserPermissions(User user)
        {
            var items = new List<Permission>();
            using (IRecordsReader dr = SqlHelper.ExecuteReader("select * from umbracoUser2NodePermission where userId = @userId order by nodeId", SqlHelper.CreateParameter("@userId", user.Id)))
            {
                while (dr.Read())
                {
                    items.Add(new Permission()
                    {
                        NodeId = dr.GetInt("nodeId"),
                        PermissionId = Convert.ToChar(dr.GetString("permission")),
                        UserId = dr.GetInt("userId")
                    });
                }
            }
            return items;
        }

        /// <summary>
        /// Returns the permissions for a node
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static IEnumerable<Permission> GetNodePermissions(CMSNode node)
        {
            var items = new List<Permission>();
            using (IRecordsReader dr = SqlHelper.ExecuteReader("select * from umbracoUser2NodePermission where nodeId = @nodeId order by nodeId", SqlHelper.CreateParameter("@nodeId", node.Id)))
            {
                while (dr.Read())
                {
                    items.Add(new Permission()
                    {
                        NodeId = dr.GetInt("nodeId"),
                        PermissionId = Convert.ToChar(dr.GetString("permission")),
                        UserId = dr.GetInt("userId")
                    });
                }
            }
            return items;
        }

        /// <summary>
        /// Delets all permissions for the node/user combination
        /// </summary>
        /// <param name="user"></param>
        /// <param name="node"></param>
        public static void DeletePermissions(User user, CMSNode node)
        {
            // delete all settings on the node for this user
            SqlHelper.ExecuteNonQuery("delete from umbracoUser2NodePermission where userId = @userId and nodeId = @nodeId",
                SqlHelper.CreateParameter("@userId", user.Id), SqlHelper.CreateParameter("@nodeId", node.Id)); 
        }

        /// <summary>
        /// deletes all permissions for the user
        /// </summary>
        /// <param name="user"></param>
        public static void DeletePermissions(User user)
        {
            // delete all settings on the node for this user
            SqlHelper.ExecuteNonQuery("delete from umbracoUser2NodePermission where userId = @userId",
                SqlHelper.CreateParameter("@userId", user.Id)); 
        }

        /// <summary>
        /// delete all permissions for this node
        /// </summary>
        /// <param name="node"></param>
        public static void DeletePermissions(CMSNode node)
        {
            
            SqlHelper.ExecuteNonQuery("delete from umbracoUser2NodePermission where nodeId = @nodeId",
                SqlHelper.CreateParameter("@nodeId", node.Id));
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
		public static void UpdateCruds(User user, CMSNode node, string permissions) 
		{
			// delete all settings on the node for this user
            DeletePermissions(user, node);

			// Loop through the permissions and create them
			foreach (char c in permissions.ToCharArray())
				MakeNew(user, node, c);
		}
	}
}