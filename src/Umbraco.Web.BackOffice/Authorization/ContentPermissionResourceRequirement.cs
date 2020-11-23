using Microsoft.AspNetCore.Authorization;

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
            PermissionToCheck = permissionToCheck;
        }

        public char PermissionToCheck { get; }
    }
}
