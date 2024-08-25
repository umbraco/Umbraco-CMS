using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace Umbraco.Cms.Api.Management.Security.Authorization.DenyLocalLogin;

/// <summary>
///     Ensures the resource cannot be accessed if <see cref="IBackOfficeExternalLoginProviders.HasDenyLocalLogin" />
///     returns <c>true</c>.
/// </summary>
public class DenyLocalLoginHandler : MustSatisfyRequirementAuthorizationHandler<DenyLocalLoginRequirement>
{
    private readonly IBackOfficeExternalLoginProviders _externalLogins;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DenyLocalLoginHandler" /> class.
    /// </summary>
    /// <param name="externalLogins">Provides access to <see cref="BackOfficeExternalLoginProvider" /> instances.</param>
    public DenyLocalLoginHandler(IBackOfficeExternalLoginProviders externalLogins)
        => _externalLogins = externalLogins;

    protected override Task<bool> IsAuthorized(AuthorizationHandlerContext context, DenyLocalLoginRequirement requirement)
    {
        var isDenied = _externalLogins.HasDenyLocalLogin();

        if (isDenied is false)
        {
            // AuthorizationPolicies.BackOfficeAccess policy adds this requirement by policy.RequireAuthenticatedUser()
            // Since we want to "allow anonymous" for some endpoints (i.e. BackOfficeController.Login()), it is necessary to succeed this requirement
            IEnumerable<DenyAnonymousAuthorizationRequirement> denyAnonymousUserRequirements = context.PendingRequirements.OfType<DenyAnonymousAuthorizationRequirement>();
            foreach (DenyAnonymousAuthorizationRequirement denyAnonymousUserRequirement in denyAnonymousUserRequirements)
            {
                context.Succeed(denyAnonymousUserRequirement);
            }
        }

        return Task.FromResult(isDenied is false);
    }
}
