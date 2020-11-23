using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using Umbraco.Web.Actions;

namespace Umbraco.Web.BackOffice.Authorization
{

    /// <summary>
    /// An authorization requirement for <see cref="ContentPermissionsResourceHandler"/>
    /// </summary>
    public class ContentPermissionsResourceRequirement : IAuthorizationRequirement
    {
        /// <summary>
        /// Create an authorization requirement for a resource
        /// </summary>
        /// <param name="permissionToCheck"></param>
        public ContentPermissionsResourceRequirement(char permissionToCheck)
        {
            PermissionsToCheck = new List<char> { permissionToCheck };
        }

        public ContentPermissionsResourceRequirement(IReadOnlyList<char> permissionToCheck)
        {
            PermissionsToCheck = permissionToCheck;
        }

        public ContentPermissionsResourceRequirement(int nodeId, IReadOnlyList<char> permissionToCheck)
        {
            NodeId = nodeId;
            PermissionsToCheck = permissionToCheck;
        }

        public int? NodeId { get; }
        public IReadOnlyList<char> PermissionsToCheck { get; }
    }
}
