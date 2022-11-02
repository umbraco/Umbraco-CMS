using System.Security.Claims;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Web.BackOffice.Security;
using Umbraco.Extensions;
using Umbraco.New.Cms.Web.Common.Routing;

namespace Umbraco.Cms.ManagementApi.Controllers.Security;

[ApiController]
[VersionedApiBackOfficeRoute(Paths.BackOfficeApiEndpointTemplate)]
[OpenApiTag("Security")]
public class BackOfficeController : ManagementApiControllerBase
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IBackOfficeSignInManager _backOfficeSignInManager;
    private readonly IBackOfficeUserManager _backOfficeUserManager;

    public BackOfficeController(IHttpContextAccessor httpContextAccessor, IBackOfficeSignInManager backOfficeSignInManager, IBackOfficeUserManager backOfficeUserManager)
    {
        _httpContextAccessor = httpContextAccessor;
        _backOfficeSignInManager = backOfficeSignInManager;
        _backOfficeUserManager = backOfficeUserManager;
    }

    [HttpGet("authorize")]
    [HttpPost("authorize")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Authorize()
    {
        HttpContext context = _httpContextAccessor.GetRequiredHttpContext();
        OpenIddictRequest? request = context.GetOpenIddictServerRequest();
        if (request == null)
        {
            return BadRequest("Unable to obtain OpenID data from the current request");
        }

        // retrieve the user principal stored in the authentication cookie.
        AuthenticateResult cookieAuthResult = await HttpContext.AuthenticateAsync(Constants.Security.BackOfficeAuthenticationType);
        if (cookieAuthResult.Succeeded && cookieAuthResult.Principal?.Identity?.Name != null)
        {
            BackOfficeIdentityUser? backOfficeUser = await _backOfficeUserManager.FindByNameAsync(cookieAuthResult.Principal.Identity.Name);
            if (backOfficeUser != null)
            {
                ClaimsPrincipal backOfficePrincipal = await _backOfficeSignInManager.CreateUserPrincipalAsync(backOfficeUser);
                backOfficePrincipal.SetClaim(OpenIddictConstants.Claims.Subject, backOfficeUser.Key.ToString());

                // TODO: it is not optimal to append all claims to the token.
                // the token size grows with each claim, although it is still smaller than the old cookie.
                // see if we can find a better way so we do not risk leaking sensitive data in bearer tokens.
                // maybe work with scopes instead?
                Claim[] backOfficeClaims = backOfficePrincipal.Claims.ToArray();
                foreach (Claim backOfficeClaim in backOfficeClaims)
                {
                    backOfficeClaim.SetDestinations(OpenIddictConstants.Destinations.AccessToken);
                }

                if (request.GetScopes().Contains(OpenIddictConstants.Scopes.OfflineAccess))
                {
                    // "offline_access" scope is required to use refresh tokens
                    backOfficePrincipal.SetScopes(OpenIddictConstants.Scopes.OfflineAccess);
                }

                return new SignInResult(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme, backOfficePrincipal);
            }
        }

        return new ChallengeResult(new[] { Constants.Security.BackOfficeAuthenticationType });
    }
}
