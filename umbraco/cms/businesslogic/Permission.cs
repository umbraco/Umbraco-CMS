using System;
using System.Collections;
using System.Collections.Specialized;
using System.Data;
using System.Runtime.CompilerServices;

using umbraco.DataLayer;

namespace umbraco.BusinessLogic
{
	/// <summary>
	/// Summary description for Permission.
	/// </summary>
	public class Permission
	{
		public Permission() 
		{
		}

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

		public static void UpdateCruds(BusinessLogic.User User, cms.businesslogic.CMSNode Node, string Permissions) 
		{
			// delete all settings on the node for this user
			SqlHelper.ExecuteNonQuery("delete from umbracoUser2NodePermission where userId = @userId and nodeId = @nodeId", SqlHelper.CreateParameter("@userId", User.Id), SqlHelper.CreateParameter("@nodeId", Node.Id)); 

			// Loop through the permissions and create them
			foreach (char c in Permissions.ToCharArray())
				MakeNew(User, Node, c);
		}
	}
}