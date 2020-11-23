using Microsoft.AspNetCore.Authorization;

namespace Umbraco.Web.BackOffice.Authorization
{

    /// <summary>
    /// An authorization requirement for <see cref="ContentPermissionQueryStringHandler"/>
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
        public ContentPermissionsQueryStringRequirement(char permissionToCheck, string[] paramNames)
        {
            QueryStringNames = paramNames;
            PermissionToCheck = permissionToCheck;
        }

        public int? NodeId { get; }
        public string[] QueryStringNames { get; }
        public char PermissionToCheck { get; }
    }
}
