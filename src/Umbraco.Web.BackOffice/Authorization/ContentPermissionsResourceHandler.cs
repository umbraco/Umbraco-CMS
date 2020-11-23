using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using Umbraco.Core.Models;
using Umbraco.Core.Security;
using Umbraco.Core.Services;

namespace Umbraco.Web.BackOffice.Authorization
{
    /// <summary>
    /// Used to authorize if the user has the correct permission access to the content for the <see cref="IContent"/> specified
    /// </summary>
    public class ContentPermissionsResourceHandler : AuthorizationHandler<ContentPermissionsResourceRequirement, IContent>
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

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ContentPermissionsResourceRequirement requirement, IContent resource)
        {
            var permissionResult = _contentPermissions.CheckPermissions(resource,
                _backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser,
                requirement.PermissionsToCheck);

            if (permissionResult == ContentPermissions.ContentAccess.Denied)
            {
                context.Fail();
            }
            else
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
