using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using Umbraco.Core.Models;
using Umbraco.Core.Security;

namespace Umbraco.Web.BackOffice.Authorization
{
    /// <summary>
    /// Used to authorize if the user has the correct permission access to the content for the <see cref="IContent"/> specified
    /// </summary>
    public class MediaPermissionsResourceHandler : MustSatisfyRequirementAuthorizationHandler<MediaPermissionsResourceRequirement, MediaPermissionsResource>
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

        protected override Task<bool> IsAuthorized(AuthorizationHandlerContext context, MediaPermissionsResourceRequirement requirement, MediaPermissionsResource resource)
        {
            var permissionResult = resource.NodeId.HasValue
                ? _mediaPermissions.CheckPermissions(
                    _backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser,
                    resource.NodeId.Value,
                    out _)
                : _mediaPermissions.CheckPermissions(
                    resource.Media,
                    _backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser);

            return Task.FromResult(permissionResult != MediaPermissions.MediaAccess.Denied);
        }
    }
}
