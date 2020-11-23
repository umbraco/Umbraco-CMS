using Microsoft.AspNetCore.Authorization;

namespace Umbraco.Web.BackOffice.Authorization
{
    /// <summary>
    /// An authorization requirement for <see cref="MediaPermissionsResourceHandler"/>
    /// </summary>
    public class MediaPermissionsResourceRequirement : IAuthorizationRequirement
    {
        public MediaPermissionsResourceRequirement()
        {
        }

        public MediaPermissionsResourceRequirement(int nodeId)
        {
            NodeId = nodeId;
        }

        public int? NodeId { get; }
    }
}
