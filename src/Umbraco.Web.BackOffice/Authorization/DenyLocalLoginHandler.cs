using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using Umbraco.Web.BackOffice.Security;
using Umbraco.Web.Common.Security;

namespace Umbraco.Web.BackOffice.Authorization
{

    /// <summary>
    /// Ensures the resource cannot be accessed if <see cref="IBackOfficeExternalLoginProviders.HasDenyLocalLogin"/> returns true
    /// </summary>
    public class DenyLocalLoginHandler : MustSatisfyRequirementAuthorizationHandler<DenyLocalLoginRequirement>
    {
        private readonly IBackOfficeExternalLoginProviders _externalLogins;

        public DenyLocalLoginHandler(IBackOfficeExternalLoginProviders externalLogins)
        {
            _externalLogins = externalLogins;
        }

        protected override Task<bool> IsAuthorized(AuthorizationHandlerContext context, DenyLocalLoginRequirement requirement)
        {
            return Task.FromResult(!_externalLogins.HasDenyLocalLogin());
        }
    }
}
