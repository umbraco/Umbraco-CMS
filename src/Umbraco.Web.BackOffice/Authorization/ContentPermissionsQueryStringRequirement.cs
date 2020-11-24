using Microsoft.AspNetCore.Authorization;

namespace Umbraco.Web.BackOffice.Authorization
{

    /// <summary>
    /// An authorization requirement for <see cref="ContentPermissionsQueryStringHandler"/>
    /// </summary>
    public class ContentPermissionsQueryStringRequirement : IAuthorizationRequirement
    {
       
        /// <summary>
        /// Create an authorization requirement for a specific node id
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="permissionToCheck"></param>
        public ContentPermissionsQueryStringRequirement(int nodeId, char permissionToCheck)
        {
            NodeId = nodeId;
            PermissionToCheck = permissionToCheck;
        }

        /// <summary>
        /// Create an authorization requirement for a node id based on a query string parameter
        /// </summary>
        /// <param name="paramName"></param>
        /// <param name="permissionToCheck"></param>
        public ContentPermissionsQueryStringRequirement(char permissionToCheck, string paramName = "id")
        {
            QueryStringName = paramName;
            PermissionToCheck = permissionToCheck;
        }

        public int? NodeId { get; }
        public string QueryStringName { get; }
        public char PermissionToCheck { get; }
    }
}
