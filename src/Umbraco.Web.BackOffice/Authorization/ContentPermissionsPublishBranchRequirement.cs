using Microsoft.AspNetCore.Authorization;

namespace Umbraco.Web.BackOffice.Authorization
{
    /// <summary>
    /// Authorization requirement for <see cref="ContentPermissionsPublishBranchHandler"/>
    /// </summary>
    public class ContentPermissionsPublishBranchRequirement : IAuthorizationRequirement
    {
        public ContentPermissionsPublishBranchRequirement(char permission)
        {
            Permission = permission;
        }

        public char Permission { get; }
    }
}
