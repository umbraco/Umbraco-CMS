using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Umbraco.Core.Models;
using Umbraco.Core.Security;

namespace Umbraco.Web.BackOffice.Authorization
{
    /// <summary>
    /// Used to authorize if the user has the correct permission access to the content for the <see cref="IContent"/> specified.
    /// </summary>
    public class ContentPermissionsResourceHandler : MustSatisfyRequirementAuthorizationHandler<ContentPermissionsResourceRequirement, ContentPermissionsResource>
    {
        private readonly IBackOfficeSecurityAccessor _backofficeSecurityAccessor;
        private readonly ContentPermissions _contentPermissions;

        public ContentPermissionsResourceHandler(
            IBackOfficeSecurityAccessor backofficeSecurityAccessor,
            ContentPermissions contentPermissions)
        {
            _backofficeSecurityAccessor = backofficeSecurityAccessor;
            _contentPermissions = contentPermissions;
        }

        protected override Task<bool> IsAuthorized(AuthorizationHandlerContext context, ContentPermissionsResourceRequirement requirement, ContentPermissionsResource resource)
        {
            var permissionResult = resource.NodeId.HasValue
                    ? _contentPermissions.CheckPermissions(
                            resource.NodeId.Value,
                            _backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser,
                            out IContent _,
                            resource.PermissionsToCheck)
                    : _contentPermissions.CheckPermissions(
                            resource.Content,
                            _backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser,
                            resource.PermissionsToCheck);

            return Task.FromResult(permissionResult != ContentPermissions.ContentAccess.Denied);
        }
    }
}
