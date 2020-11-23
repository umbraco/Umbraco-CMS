using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using Umbraco.Web.Common.Security;

namespace Umbraco.Web.BackOffice.Authorization
{

    /// <summary>
    /// Ensures the resource cannot be accessed if <see cref="IBackOfficeExternalLoginProviders.HasDenyLocalLogin"/> returns true
    /// </summary>
    public class DenyLocalLoginHandler : AuthorizationHandler<DenyLocalLoginRequirement>
    {
        private readonly IBackOfficeExternalLoginProviders _externalLogins;

        public DenyLocalLoginHandler(IBackOfficeExternalLoginProviders externalLogins)
        {
            _externalLogins = externalLogins;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, DenyLocalLoginRequirement requirement)
        {
            if (_externalLogins.HasDenyLocalLogin())
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
