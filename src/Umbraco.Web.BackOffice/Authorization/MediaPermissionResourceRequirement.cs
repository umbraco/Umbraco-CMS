using Microsoft.AspNetCore.Authorization;

namespace Umbraco.Web.BackOffice.Authorization
{
    /// <summary>
    /// An authorization requirement for <see cref="MediaPermissionResourceHandler"/>
    /// </summary>
    public class MediaPermissionResourceRequirement : IAuthorizationRequirement
    {
        public MediaPermissionResourceRequirement()
        {
        }

        public MediaPermissionResourceRequirement(int nodeId)
        {
            NodeId = nodeId;
        }

        public int? NodeId { get; }
    }
}
