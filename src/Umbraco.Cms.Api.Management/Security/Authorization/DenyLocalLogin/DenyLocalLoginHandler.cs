using Microsoft.AspNetCore.Authorization;

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

    // FIXME: Replace with above implementation, once we have IBackOfficeExternalLoginProviders and related classes
    // moved from Umbraco.Web.Backoffice
    // FIXME: Remove [AllowAnonymous] from implementers of <see cref="SecurityControllerBase" /> when we have the proper implementation
    protected override Task<bool> IsAuthorized(AuthorizationHandlerContext context, DenyLocalLoginRequirement requirement)
        => Task.FromResult(true);
}
