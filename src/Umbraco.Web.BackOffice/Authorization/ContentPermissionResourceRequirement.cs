using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using Umbraco.Web.Actions;

namespace Umbraco.Web.BackOffice.Authorization
{

    /// <summary>
    /// An authorization requirement for <see cref="ContentPermissionResourceHandler"/>
    /// </summary>
    public class ContentPermissionResourceRequirement : IAuthorizationRequirement
    {
        /// <summary>
        /// Create an authorization requirement for a resource
        /// </summary>
        /// <param name="permissionToCheck"></param>
        public ContentPermissionResourceRequirement(char permissionToCheck)
        {
            PermissionsToCheck = new List<char> { permissionToCheck };
        }

        public ContentPermissionResourceRequirement(IReadOnlyList<char> permissionToCheck)
        {
            PermissionsToCheck = permissionToCheck;
        }

        public ContentPermissionResourceRequirement(int nodeId, IReadOnlyList<char> permissionToCheck)
        {
            NodeId = nodeId;
            PermissionsToCheck = permissionToCheck;
        }

        public int? NodeId { get; }
        public IReadOnlyList<char> PermissionsToCheck { get; }
    }
}
