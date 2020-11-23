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
    public class SectionHandler : AuthorizationHandler<SectionRequirement>
    {
        private readonly IBackOfficeSecurityAccessor _backofficeSecurityAccessor;

        public SectionHandler(IBackOfficeSecurityAccessor backofficeSecurityAccessor)
        {
            _backofficeSecurityAccessor = backofficeSecurityAccessor;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, SectionRequirement requirement)
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

        private bool IsAuthorized(SectionRequirement requirement)
        {
            var authorized = _backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser != null
                             && requirement.SectionAliases.Any(app => _backofficeSecurityAccessor.BackOfficeSecurity.UserHasSectionAccess(
                                 app, _backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser));

            return authorized;
        }
    }
}
