using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace Umbraco.Cms.Api.Management.Security.Authorization.DenyLocalLogin;

/// <summary>
///     Ensures the resource cannot be accessed if <see cref="IBackOfficeExternalLoginProviders.HasDenyLocalLogin" />
///     returns <c>true</c>.
/// </summary>
public class DenyLocalLoginHandler : MustSatisfyRequirementAuthorizationHandler<DenyLocalLoginRequirement>
{
    // private readonly IBackOfficeExternalLoginProviders _externalLogins;
    //
    // /// <summary>
    // ///     Initializes a new instance of the <see cref="DenyLocalLoginHandler" /> class.
    // /// </summary>
    // /// <param name="externalLogins">Provides access to <see cref="BackOfficeExternalLoginProvider" /> instances.</param>
    // public DenyLocalLoginHandler(IBackOfficeExternalLoginProviders externalLogins)
    //     => _externalLogins = externalLogins;
    //
    // /// <inheritdoc />
    // protected override Task<bool> IsAuthorized(AuthorizationHandlerContext context, DenyLocalLoginRequirement requirement)
    //     => Task.FromResult(!_externalLogins.HasDenyLocalLogin());

    // FIXME: Replace the value of isDenied with above implementation, once we have IBackOfficeExternalLoginProviders and related classes
    // moved from Umbraco.Web.Backoffice
    // FIXME: Remove [AllowAnonymous] from implementers of <see cref="SecurityControllerBase" /> and in <see cref="VerifyInviteUserController" /> when we have the proper implementation
    protected override Task<bool> IsAuthorized(AuthorizationHandlerContext context, DenyLocalLoginRequirement requirement)
    {
        // Some logic here - for now we will always authorize successfully
        var isDenied = true;

        if (isDenied)
        {
            // Now allow anonymous - necessary for some of the endpoints (BackOfficeController.Login())
            var denyAnonymousUserRequirements = context.PendingRequirements.OfType<DenyAnonymousAuthorizationRequirement>();
            foreach (var denyAnonymousUserRequirement in denyAnonymousUserRequirements)
            {
                context.Succeed(denyAnonymousUserRequirement);
            }
        }

        return Task.FromResult(isDenied);
    }
}
