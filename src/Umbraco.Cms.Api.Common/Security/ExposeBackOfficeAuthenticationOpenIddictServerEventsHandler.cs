using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using OpenIddict.Server;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Security;

public class ExposeBackOfficeAuthenticationOpenIddictServerEventsHandler : IOpenIddictServerHandler<OpenIddictServerEvents.GenerateTokenContext>,
    IOpenIddictServerHandler<OpenIddictServerEvents.ApplyRevocationResponseContext>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly string[] _claimTypes;
    private readonly TimeSpan _timeOut;

    public ExposeBackOfficeAuthenticationOpenIddictServerEventsHandler(
        IHttpContextAccessor httpContextAccessor,
        IOptions<GlobalSettings> globalSettings,
        IOptions<BackOfficeIdentityOptions> backOfficeIdentityOptions)
    {
        _httpContextAccessor = httpContextAccessor;
        _timeOut = globalSettings.Value.TimeOut;

        // these are the type identifiers for the claims required by the principal
        // for the custom authentication scheme 
        _claimTypes =
        [
            backOfficeIdentityOptions.Value.ClaimsIdentity.UserIdClaimType,
            backOfficeIdentityOptions.Value.ClaimsIdentity.UserNameClaimType,
            Core.Constants.Security.AllowedApplicationsClaimType,
        ];
    }

    // event handler for when access tokens are generated (created or refreshed)
    public async ValueTask HandleAsync(OpenIddictServerEvents.GenerateTokenContext context)
    {
        // only proceed if this is a back-office sign-in
        if (context.Principal.Identity?.AuthenticationType != Core.Constants.Security.BackOfficeAuthenticationType)
        {
            return;
        }

        // create a new principal with the claims from the authenticated principal 
        var principal = new ClaimsPrincipal(
            new ClaimsIdentity(
                context.Principal.Claims.Where(claim => _claimTypes.Contains(claim.Type)),
                Core.Constants.Security.BackOfficeExposedAuthenticationType
            )
        );

        // sign-in the new principal for the custom authentication scheme
        await _httpContextAccessor
            .GetRequiredHttpContext()
            .SignInAsync(Core.Constants.Security.BackOfficeExposedAuthenticationType, principal, GetAuthenticationProperties());
    }

    // event handler for when access tokens are revoked
    public async ValueTask HandleAsync(OpenIddictServerEvents.ApplyRevocationResponseContext context)
        => await _httpContextAccessor
            .GetRequiredHttpContext()
            .SignOutAsync(Core.Constants.Security.BackOfficeExposedAuthenticationType, GetAuthenticationProperties());

    private AuthenticationProperties GetAuthenticationProperties()
        => new()
        {
            IsPersistent = true,
            IssuedUtc = DateTimeOffset.UtcNow,
            ExpiresUtc = DateTimeOffset.UtcNow.Add(_timeOut)
        };
}
