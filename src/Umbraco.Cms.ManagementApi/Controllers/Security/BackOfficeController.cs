﻿using System.Security.Claims;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
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
using Umbraco.New.Cms.Web.Common.Routing;
using SignInResult = Microsoft.AspNetCore.Mvc.SignInResult;
using IdentitySignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace Umbraco.Cms.ManagementApi.Controllers.Security;

[ApiController]
[VersionedApiBackOfficeRoute(Paths.BackOfficeApiEndpointTemplate)]
[ApiExplorerSettings(GroupName = "Security")]
public class BackOfficeController : ManagementApiControllerBase
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
        AuthenticateResult cookieAuthResult = await HttpContext.AuthenticateAsync(Constants.Security.BackOfficeAuthenticationType);
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

                // sign in the backoffice user associated with the login provider and unique provider key
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

    private static IActionResult DefaultChallengeResult() => new ChallengeResult(Constants.Security.BackOfficeAuthenticationType);
}
