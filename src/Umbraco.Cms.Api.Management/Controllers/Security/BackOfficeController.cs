using System.Security.Claims;
using Asp.Versioning;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Web.BackOffice.Security;
using Umbraco.Extensions;
using Umbraco.Cms.Api.Management.Routing;
using SignInResult = Microsoft.AspNetCore.Mvc.SignInResult;
using IdentitySignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace Umbraco.Cms.Api.Management.Controllers.Security;

[ApiVersion("1.0")]
[ApiController]
[VersionedApiBackOfficeRoute(Common.Security.Paths.BackOfficeApi.EndpointTemplate)]
public class BackOfficeController : SecurityControllerBase
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IBackOfficeSignInManager _backOfficeSignInManager;
    private readonly IBackOfficeUserManager _backOfficeUserManager;
    private readonly IOptions<SecuritySettings> _securitySettings;

    public BackOfficeController(
        IHttpContextAccessor httpContextAccessor,
        IBackOfficeSignInManager backOfficeSignInManager,
        IBackOfficeUserManager backOfficeUserManager,
        IOptions<SecuritySettings> securitySettings)
    {
        _httpContextAccessor = httpContextAccessor;
        _backOfficeSignInManager = backOfficeSignInManager;
        _backOfficeUserManager = backOfficeUserManager;
        _securitySettings = securitySettings;
    }

    // FIXME: this is a temporary solution to get the new backoffice auth rolling.
    //        once the old backoffice auth is no longer necessary, clean this up and merge with 2FA handling etc.
    [AllowAnonymous]
    [HttpPost("login")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Login(LoginRequestModel model)
    {
        var validated = await _backOfficeUserManager.ValidateCredentialsAsync(model.Username, model.Password);
        if (validated is false)
        {
            return Unauthorized();
        }

        var claims = new List<Claim> { new(ClaimTypes.Name, model.Username) };
        var claimsIdentity = new ClaimsIdentity(claims, Constants.Security.NewBackOfficeAuthenticationType);
        await HttpContext.SignInAsync(Constants.Security.NewBackOfficeAuthenticationType, new ClaimsPrincipal(claimsIdentity));

        return Ok();
    }

    public class LoginRequestModel
    {
        public required string Username { get; init; }

        public required string Password { get; init; }
    }

    [AllowAnonymous]
    [HttpGet("authorize")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Authorize()
    {
        HttpContext context = _httpContextAccessor.GetRequiredHttpContext();
        OpenIddictRequest? request = context.GetOpenIddictServerRequest();
        if (request == null)
        {
            return BadRequest("Unable to obtain OpenID data from the current request");
        }

        // make sure we keep member authentication away from this endpoint
        if (request.ClientId is Constants.OAuthClientIds.Member)
        {
            return BadRequest("The specified client ID cannot be used here.");
        }

        return request.IdentityProvider.IsNullOrWhiteSpace()
            ? await AuthorizeInternal(request)
            : await AuthorizeExternal(request);
    }

    private async Task<IActionResult> AuthorizeInternal(OpenIddictRequest request)
    {
        // TODO: ensure we handle sign-in notifications for internal logins.
        // when the new login screen is implemented for internal logins, make sure it still handles
        // user sign-in notifications (calls BackOfficeSignInManager.HandleSignIn) as part of the
        // sign-in process
        // for future reference, notifications are already handled for the external login flow by
        // by calling BackOfficeSignInManager.ExternalLoginSignInAsync

        // retrieve the user principal stored in the authentication cookie.
        AuthenticateResult cookieAuthResult = await HttpContext.AuthenticateAsync(Constants.Security.NewBackOfficeAuthenticationType);
        var userName = cookieAuthResult.Succeeded
            ? cookieAuthResult.Principal?.Identity?.Name
            : null;

        if (userName != null)
        {
            BackOfficeIdentityUser? backOfficeUser = await _backOfficeUserManager.FindByNameAsync(userName);
            if (backOfficeUser != null)
            {
                return await SignInBackOfficeUser(backOfficeUser, request);
            }
        }

        return DefaultChallengeResult();
    }

    private async Task<IActionResult> AuthorizeExternal(OpenIddictRequest request)
    {
        var provider = request.IdentityProvider ?? throw new ArgumentException("No identity provider found in request", nameof(request));

        ExternalLoginInfo? loginInfo = await _backOfficeSignInManager.GetExternalLoginInfoAsync();
        if (loginInfo?.Principal != null)
        {
            IdentitySignInResult result = await _backOfficeSignInManager.ExternalLoginSignInAsync(loginInfo, false, _securitySettings.Value.UserBypassTwoFactorForExternalLogins);

            if (result.Succeeded)
            {
                // Update any authentication tokens if succeeded
                await _backOfficeSignInManager.UpdateExternalAuthenticationTokensAsync(loginInfo);

                // sign in the backoffice user associated with the login provider and unique provider id
                BackOfficeIdentityUser? backOfficeUser = await _backOfficeUserManager.FindByLoginAsync(loginInfo.LoginProvider, loginInfo.ProviderKey);
                if (backOfficeUser != null)
                {
                    return await SignInBackOfficeUser(backOfficeUser, request);
                }
            }
            else
            {
                // avoid infinite auth loops when something fails by performing the default challenge (default login screen)
                return DefaultChallengeResult();
            }
        }

        AuthenticationProperties properties = _backOfficeSignInManager.ConfigureExternalAuthenticationProperties(provider, null);
        return new ChallengeResult(provider, properties);
    }

    private async Task<IActionResult> SignInBackOfficeUser(BackOfficeIdentityUser backOfficeUser, OpenIddictRequest request)
    {
        ClaimsPrincipal backOfficePrincipal = await _backOfficeSignInManager.CreateUserPrincipalAsync(backOfficeUser);
        backOfficePrincipal.SetClaim(OpenIddictConstants.Claims.Subject, backOfficeUser.Key.ToString());

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

    private static IActionResult DefaultChallengeResult() => new ChallengeResult(Constants.Security.NewBackOfficeAuthenticationType);
}
