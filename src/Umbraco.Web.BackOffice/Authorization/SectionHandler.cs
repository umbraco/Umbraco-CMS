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
    public class SectionHandler : MustSatisfyRequirementAuthorizationHandler<SectionRequirement>
    {
        private readonly IBackOfficeSecurityAccessor _backofficeSecurityAccessor;

        public SectionHandler(IBackOfficeSecurityAccessor backofficeSecurityAccessor)
        {
            _backofficeSecurityAccessor = backofficeSecurityAccessor;
        }

        protected override Task<bool> IsAuthorized(AuthorizationHandlerContext context, SectionRequirement requirement)
        {
            var authorized = _backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser != null
                             && requirement.SectionAliases.Any(app => _backofficeSecurityAccessor.BackOfficeSecurity.UserHasSectionAccess(
                                 app, _backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser));

            return Task.FromResult(authorized);
        }
    }
}
