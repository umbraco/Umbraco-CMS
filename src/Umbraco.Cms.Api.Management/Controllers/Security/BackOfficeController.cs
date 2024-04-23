﻿using System.Security.Claims;
using Asp.Versioning;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Api.Management.Security;
using Umbraco.Cms.Api.Management.Services;
using Umbraco.Cms.Api.Management.ViewModels.Security;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Cms.Web.Common.Security;
using Umbraco.Extensions;
using IdentitySignInResult = Microsoft.AspNetCore.Identity.SignInResult;
using SignInResult = Microsoft.AspNetCore.Mvc.SignInResult;

namespace Umbraco.Cms.Api.Management.Controllers.Security;

[ApiVersion("1.0")]
[VersionedApiBackOfficeRoute(Common.Security.Paths.BackOfficeApi.EndpointTemplate)]
[ApiExplorerSettings(IgnoreApi = true)]
public class BackOfficeController : SecurityControllerBase
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IBackOfficeSignInManager _backOfficeSignInManager;
    private readonly IBackOfficeUserManager _backOfficeUserManager;
    private readonly IOptions<SecuritySettings> _securitySettings;
    private readonly ILogger<BackOfficeController> _logger;
    private readonly IBackOfficeTwoFactorOptions _backOfficeTwoFactorOptions;
    private readonly IUserTwoFactorLoginService _userTwoFactorLoginService;
    private readonly IBackOfficeExternalLoginService _externalLoginService;

    public BackOfficeController(
        IHttpContextAccessor httpContextAccessor,
        IBackOfficeSignInManager backOfficeSignInManager,
        IBackOfficeUserManager backOfficeUserManager,
        IOptions<SecuritySettings> securitySettings,
        ILogger<BackOfficeController> logger,
        IBackOfficeTwoFactorOptions backOfficeTwoFactorOptions,
        IUserTwoFactorLoginService userTwoFactorLoginService,
        IBackOfficeExternalLoginService externalLoginService)
    {
        _httpContextAccessor = httpContextAccessor;
        _backOfficeSignInManager = backOfficeSignInManager;
        _backOfficeUserManager = backOfficeUserManager;
        _securitySettings = securitySettings;
        _logger = logger;
        _backOfficeTwoFactorOptions = backOfficeTwoFactorOptions;
        _userTwoFactorLoginService = userTwoFactorLoginService;
        _externalLoginService = externalLoginService;
    }

    [HttpPost("login")]
    [MapToApiVersion("1.0")]
    [Authorize(Policy = AuthorizationPolicies.DenyLocalLoginIfConfigured)]
    public async Task<IActionResult> Login(CancellationToken cancellationToken, LoginRequestModel model)
    {
        var validated = await _backOfficeUserManager.ValidateCredentialsAsync(model.Username, model.Password);
        if (validated is false)
        {
            return Unauthorized();
        }

        IdentitySignInResult result = await _backOfficeSignInManager.PasswordSignInAsync(
            model.Username, model.Password, true, true);

        if (result.IsNotAllowed)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ProblemDetailsBuilder()
                .WithTitle("User is not allowed")
                .WithDetail("The operation is not allowed on the user")
                .Build());
        }

        if (result.IsLockedOut)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ProblemDetailsBuilder()
                .WithTitle("User is locked")
                .WithDetail("The user is locked, and need to be unlocked before more login attempts can be executed.")
                .Build());
        }

        if(result.RequiresTwoFactor)
        {
            string? twofactorView = _backOfficeTwoFactorOptions.GetTwoFactorView(model.Username);
            BackOfficeIdentityUser? attemptingUser = await _backOfficeUserManager.FindByNameAsync(model.Username);
            IEnumerable<string> enabledProviders = (await _userTwoFactorLoginService.GetProviderNamesAsync(attemptingUser!.Key)).Result.Where(x=>x.IsEnabledOnUser).Select(x=>x.ProviderName);
            return StatusCode(StatusCodes.Status402PaymentRequired, new RequiresTwoFactorResponseModel()
            {
                TwoFactorLoginView = twofactorView,
                EnabledTwoFactorProviderNames = enabledProviders
            });
        }
        return Ok();
    }

    [AllowAnonymous]
    [HttpPost("verify-2fa")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Verify2FACode(CancellationToken cancellationToken, Verify2FACodeModel model)
    {
        if (ModelState.IsValid == false)
        {
            return BadRequest();
        }

        BackOfficeIdentityUser? user = await _backOfficeSignInManager.GetTwoFactorAuthenticationUserAsync();
        if (user is null)
        {
            return StatusCode(StatusCodes.Status401Unauthorized, new ProblemDetailsBuilder()
                .WithTitle("No user found")
                .Build());
        }

        IdentitySignInResult result =
            await _backOfficeSignInManager.TwoFactorSignInAsync(model.Provider, model.Code, model.IsPersistent, model.RememberClient);
        if (result.Succeeded)
        {
            return Ok();
        }

        if (result.IsLockedOut)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ProblemDetailsBuilder()
                .WithTitle("User is locked.")
                .Build());
        }

        if (result.IsNotAllowed)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ProblemDetailsBuilder()
                .WithTitle("User is not allowed")
                .Build());
        }

        return StatusCode(StatusCodes.Status400BadRequest, new ProblemDetailsBuilder()
            .WithTitle("Invalid code")
            .Build());
    }

    [AllowAnonymous]
    [HttpGet("authorize")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Authorize(CancellationToken cancellationToken)
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

    [AllowAnonymous]
    [HttpGet("signout")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Signout(CancellationToken cancellationToken)
    {
        var userName = await GetUserNameFromAuthCookie();

        await _backOfficeSignInManager.SignOutAsync();

        _logger.LogInformation("User {UserName} from IP address {RemoteIpAddress} has logged out",
            userName ?? "UNKNOWN", HttpContext.Connection.RemoteIpAddress);

        // Returning a SignOutResult will ask OpenIddict to redirect the user agent
        // to the post_logout_redirect_uri specified by the client application.
        return SignOut(Constants.Security.BackOfficeAuthenticationType, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    /// <summary>
    ///     Called when a user links an external login provider in the back office
    /// </summary>
    /// <param name="provider"></param>
    /// <returns></returns>
    [HttpGet("link-login")]
    [AllowAnonymous]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> LinkLogin(string provider)
    {
        var cookieAuthenticatedUserAttempt =
            await HttpContext.AuthenticateAsync(Constants.Security.BackOfficeAuthenticationType);

        if (cookieAuthenticatedUserAttempt.Succeeded == false)
        {
            return Redirect(_securitySettings.Value.BackOfficeHost + _securitySettings.Value.AuthorizeCallbackErrorPathName.AppendQueryStringToUrl(
                "flow=link-login",
                "status=unauthorized"));
        }

        BackOfficeIdentityUser? user = await _backOfficeUserManager.GetUserAsync(cookieAuthenticatedUserAttempt.Principal);
        if (user == null)
        {
            return Redirect(_securitySettings.Value.BackOfficeHost + _securitySettings.Value.AuthorizeCallbackErrorPathName.AppendQueryStringToUrl(
                "flow=link-login",
                "status=user-not-found"));
        }

        // Request a redirect to the external login provider to link a login for the current user
        var redirectUrl = Url.Action(nameof(ExternalLinkLoginCallback), this.GetControllerName());

        // Configures the redirect URL and user identifier for the specified external login including xsrf data
        AuthenticationProperties properties =
            _backOfficeSignInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl, user.Id);

        return Challenge(properties, provider);
    }

    /// <summary>
    ///     Callback path when the user initiates a link login request from the back office to the external provider from the
    ///     <see cref="LinkLogin(string)" /> action
    /// </summary>
    /// <remarks>
    ///     An example of this is here
    ///     https://github.com/dotnet/aspnetcore/blob/main/src/Identity/samples/IdentitySample.Mvc/Controllers/AccountController.cs#L155
    ///     which this is based on
    /// </remarks>
    [HttpGet("ExternalLinkLoginCallback")]
    [AllowAnonymous]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> ExternalLinkLoginCallback()
    {
        var cookieAuthenticatedUserAttempt =
            await HttpContext.AuthenticateAsync(Constants.Security.BackOfficeAuthenticationType);

        if (cookieAuthenticatedUserAttempt.Succeeded == false)
        {
            return Redirect(_securitySettings.Value.BackOfficeHost + _securitySettings.Value.AuthorizeCallbackErrorPathName.AppendQueryStringToUrl(
                "flow=external-login-callback",
                "status=unauthorized"));
        }

        BackOfficeIdentityUser? user = await _backOfficeUserManager.GetUserAsync(cookieAuthenticatedUserAttempt.Principal);
        if (user == null)
        {
            return Redirect(_securitySettings.Value.BackOfficeHost + _securitySettings.Value.AuthorizeCallbackErrorPathName.AppendQueryStringToUrl(
                "flow=external-login-callback",
                "status=user-not-found"));
        }

        ExternalLoginInfo? info =
            await _backOfficeSignInManager.GetExternalLoginInfoAsync();

        if (info == null)
        {
            return Redirect(_securitySettings.Value.BackOfficeHost + _securitySettings.Value.AuthorizeCallbackErrorPathName.AppendQueryStringToUrl(
                "flow=external-login-callback",
                "status=external-info-not-found"));
        }

        IdentityResult addLoginResult = await _backOfficeUserManager.AddLoginAsync(user, info);
        if (addLoginResult.Succeeded)
        {
            // Update any authentication tokens if succeeded
            await _backOfficeSignInManager.UpdateExternalAuthenticationTokensAsync(info);
            return Redirect(_securitySettings.Value.BackOfficeHost + _securitySettings.Value.AuthorizeCallbackPathName);
        }

        // Add errors and redirect for it to be displayed
        // TempData[ViewDataExtensions.TokenExternalSignInError] = addLoginResult.Errors;
        // return RedirectToLogin(new { flow = "external-login", status = "failed", logout = "true" });
        // todo
        return Redirect(_securitySettings.Value.BackOfficeHost + _securitySettings.Value.AuthorizeCallbackErrorPathName.AppendQueryStringToUrl(
            "flow=external-login-callback",
            "status=failed"));
    }

    // todo cleanup unhappy responses
    [HttpPost("unlink-login")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> PostUnLinkLogin(UnLinkLoginRequestModel unlinkLoginRequestModel)
    {
        Attempt<ExternalLoginOperationStatus> unlinkResult = await _externalLoginService.UnLinkLogin(
            User,
            unlinkLoginRequestModel.LoginProvider,
            unlinkLoginRequestModel.ProviderKey);

        if (unlinkResult.Success)
        {
            return Ok();
        }

        return OperationStatusResult(unlinkResult.Result, problemDetailsBuilder => unlinkResult.Result switch
        {
            ExternalLoginOperationStatus.UserNotFound => Unauthorized(problemDetailsBuilder
                .WithTitle("User not found")
                .Build()),
            ExternalLoginOperationStatus.IdentityNotFound => BadRequest(problemDetailsBuilder
                .WithTitle("Missing identity")
                .Build()),
            ExternalLoginOperationStatus.AuthenticationSchemeNotFound => BadRequest(problemDetailsBuilder
                .WithTitle("Authentication scheme not found")
                .WithDetail($"Could not find the authentication scheme for the supplied provider")
                .Build()),
            ExternalLoginOperationStatus.AuthenticationOptionsNotFound => BadRequest(problemDetailsBuilder
                .WithTitle("Missing Authentication options")
                .WithDetail($"Could not find external authentication options the supplied provider")
                .Build()),
            ExternalLoginOperationStatus.UnlinkingDisabled => BadRequest(problemDetailsBuilder
                .WithTitle("Unlinking disabled")
                .WithDetail($"Manual linking is disabled for the supplied provider")
                .Build()),
            ExternalLoginOperationStatus.InvalidProviderKey => BadRequest(problemDetailsBuilder
                .WithTitle("Unlinking failed")
                .WithDetail("Could not match ProviderKey to the supplied provider")
                .Build()),
            _ => throw new ArgumentOutOfRangeException()
        });
    }

    /// <summary>
    ///     Retrieve the user principal stored in the authentication cookie.
    /// </summary>
    private async Task<string?> GetUserNameFromAuthCookie()
    {
        AuthenticateResult cookieAuthResult = await HttpContext.AuthenticateAsync(Constants.Security.BackOfficeAuthenticationType);
        return cookieAuthResult.Succeeded
            ? cookieAuthResult.Principal?.Identity?.Name
            : null;
    }

    private async Task<IActionResult> AuthorizeInternal(OpenIddictRequest request)
    {
        // TODO: ensure we handle sign-in notifications for internal logins.
        // when the new login screen is implemented for internal logins, make sure it still handles
        // user sign-in notifications (calls BackOfficeSignInManager.HandleSignIn) as part of the
        // sign-in process
        // for future reference, notifications are already handled for the external login flow by
        // by calling BackOfficeSignInManager.ExternalLoginSignInAsync

        var userName = await GetUserNameFromAuthCookie();

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
