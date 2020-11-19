using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Threading.Tasks;
using Umbraco.Core.Security;

namespace Umbraco.Web.BackOffice.Authorization
{

    /// <summary>
    /// Ensures that the current user has access to the section
    /// </summary>
    /// <remarks>
    /// The user only needs access to one of the sections specified, not all of the sections.
    /// </remarks>
    public class UmbracoSectionAuthorizeHandler : AuthorizationHandler<SectionAliasesRequirement>
    {
        private readonly IBackOfficeSecurityAccessor _backofficeSecurityAccessor;

        public UmbracoSectionAuthorizeHandler(IBackOfficeSecurityAccessor backofficeSecurityAccessor)
        {
            _backofficeSecurityAccessor = backofficeSecurityAccessor;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, SectionAliasesRequirement requirement)
        {
            if (IsAuthorized(requirement))
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }

            return Task.CompletedTask;
        }

        private bool IsAuthorized(SectionAliasesRequirement requirement)
        {
            var authorized = _backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser != null
                             && requirement.SectionAliases.Any(app => _backofficeSecurityAccessor.BackOfficeSecurity.UserHasSectionAccess(
                                 app, _backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser));

            return authorized;
        }
    }
}
