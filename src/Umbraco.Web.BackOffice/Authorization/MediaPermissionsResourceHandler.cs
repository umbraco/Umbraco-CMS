using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using Umbraco.Core.Models;
using Umbraco.Core.Security;

namespace Umbraco.Web.BackOffice.Authorization
{
    /// <summary>
    /// Used to authorize if the user has the correct permission access to the content for the <see cref="IContent"/> specified
    /// </summary>
    public class MediaPermissionsResourceHandler : MustSatisfyRequirementAuthorizationHandler<MediaPermissionsResourceRequirement, IMedia>
    {
        private readonly IBackOfficeSecurityAccessor _backofficeSecurityAccessor;
        private readonly MediaPermissions _mediaPermissions;

        public MediaPermissionsResourceHandler(
            IBackOfficeSecurityAccessor backofficeSecurityAccessor,
            MediaPermissions mediaPermissions)
        {
            _backofficeSecurityAccessor = backofficeSecurityAccessor;
            _mediaPermissions = mediaPermissions;
        }

        protected override Task<bool> IsAuthorized(AuthorizationHandlerContext context, MediaPermissionsResourceRequirement requirement, IMedia resource)
        {
            var permissionResult = MediaPermissions.MediaAccess.NotFound;

            if (resource != null)
            {
                permissionResult = _mediaPermissions.CheckPermissions(
                    resource,
                    _backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser);
            }
            else if (requirement.NodeId.HasValue)
            {
                permissionResult = _mediaPermissions.CheckPermissions(
                    _backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser,
                    requirement.NodeId.Value,
                    out _);
            }

            return Task.FromResult(permissionResult != MediaPermissions.MediaAccess.Denied);
        }
    }
}
