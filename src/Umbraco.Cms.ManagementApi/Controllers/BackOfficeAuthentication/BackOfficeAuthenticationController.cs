using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NSwag.Annotations;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using OpenIddict.Validation.AspNetCore;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Web.BackOffice.Security;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;
using Umbraco.New.Cms.Web.Common.Routing;

namespace Umbraco.Cms.ManagementApi.Controllers.BackOfficeAuthentication;

[ApiController]
[BackOfficeRoute("api/v{version:apiVersion}/back-office-authentication")]
[OpenApiTag("BackOfficeAuthentication")]
public class BackOfficeAuthenticationController : ManagementApiControllerBase
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IBackOfficeSignInManager _backOfficeSignInManager;
    private readonly IBackOfficeUserManager _backOfficeUserManager;

    public BackOfficeAuthenticationController(IHttpContextAccessor httpContextAccessor, IBackOfficeSignInManager backOfficeSignInManager, IBackOfficeUserManager backOfficeUserManager)
    {
        _httpContextAccessor = httpContextAccessor;
        _backOfficeSignInManager = backOfficeSignInManager;
        _backOfficeUserManager = backOfficeUserManager;
    }

    [HttpPost("authorize")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Authorize()
    {
        HttpContext context = _httpContextAccessor.GetRequiredHttpContext();
        OpenIddictRequest request = context.GetOpenIddictServerRequest() ?? throw new ApplicationException("TODO: something descriptive");

        if (request.Username != null && request.Password != null)
        {
            Microsoft.AspNetCore.Identity.SignInResult result = await _backOfficeSignInManager.PasswordSignInAsync(request.Username, request.Password, true, true);
            if (result.Succeeded)
            {
                // TODO: what does this return if username foes not exist? shouldn't be possible, but hey...
                BackOfficeIdentityUser backOfficeUser = await _backOfficeUserManager.FindByNameAsync(request.Username);
                ClaimsPrincipal backOfficePrincipal = await _backOfficeSignInManager.CreateUserPrincipalAsync(backOfficeUser);
                backOfficePrincipal.SetClaim(OpenIddictConstants.Claims.Subject, backOfficeUser.Id);

                // TODO: not optimal at all, see if we can find a way that doesn't require us passing every last claim to the token
                Claim[] backOfficeClaims = backOfficePrincipal.Claims.ToArray();
                foreach (Claim backOfficeClaim in backOfficeClaims)
                {
                    backOfficeClaim.SetDestinations(OpenIddictConstants.Destinations.AccessToken);
                }

                return new SignInResult(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme, backOfficePrincipal);
            }
        }

        int.TryParse(request["hardcoded_identity_id"]?.ToString(), out var identifier);
        if (identifier is not (1 or 2))
        {
            return new ChallengeResult(
                new[] { OpenIddictServerAspNetCoreDefaults.AuthenticationScheme },
                new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidRequest,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The specified hardcoded identity is invalid."
                }));
        }

        // Create a new identity and populate it based on the specified hardcoded identity identifier.
        var identity = new ClaimsIdentity(TokenValidationParameters.DefaultAuthenticationType);
        identity.AddClaim(new Claim(OpenIddictConstants.Claims.Subject, identifier.ToString(CultureInfo.InvariantCulture)));
        identity.AddClaim(new Claim(OpenIddictConstants.Claims.Name, identifier switch
        {
            1 => "Alice",
            2 => "Bob",
            _ => throw new InvalidOperationException()
        }).SetDestinations(OpenIddictConstants.Destinations.AccessToken));

        var principal = new ClaimsPrincipal(identity);

        principal.SetScopes(identifier switch
        {
            1 => request.GetScopes(),
            2 => new[] { "api1" }.Intersect(request.GetScopes()),
            _ => throw new InvalidOperationException()
        });

        return new SignInResult(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme, principal);
    }

    [HttpGet("test")]
    [MapToApiVersion("1.0")]
    public IActionResult Test() => Ok("Hello");

    [HttpGet("api1")]
    [MapToApiVersion("1.0")]
    [Authorize("can_use_api_1", AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    public IActionResult Api1() => Ok($"API1 response: {User.Identity!.Name} has scopes {string.Join(", ", User.GetScopes())}");

    [HttpGet("api2")]
    [MapToApiVersion("1.0")]
    // [Authorize("can_use_api_2", AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    [Authorize(Policy = AuthorizationPolicies.TreeAccessDocumentTypes)]
    public IActionResult Api2() => Ok($"API2 response: {User.Identity!.Name} has scopes {string.Join(", ", User.GetScopes())}");
}
