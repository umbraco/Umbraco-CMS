using System.Security.Claims;
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
    private static long? _loginDurationAverage;

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IBackOfficeSignInManager _backOfficeSignInManager;
    private readonly IBackOfficeUserManager _backOfficeUserManager;
    private readonly IOptions<SecuritySettings> _securitySettings;
    private readonly ILogger<BackOfficeController> _logger;
    private readonly IBackOfficeTwoFactorOptions _backOfficeTwoFactorOptions;
    private readonly IUserTwoFactorLoginService _userTwoFactorLoginService;
    private readonly IBackOfficeExternalLoginService _externalLoginService;
    private readonly IBackOfficeUserClientCredentialsManager _backOfficeUserClientCredentialsManager;

    private const string RedirectFlowParameter = "flow";
    private const string RedirectStatusParameter = "status";
    private const string RedirectErrorCodeParameter = "errorCode";

    public BackOfficeController(
        IHttpContextAccessor httpContextAccessor,
        IBackOfficeSignInManager backOfficeSignInManager,
        IBackOfficeUserManager backOfficeUserManager,
        IOptions<SecuritySettings> securitySettings,
        ILogger<BackOfficeController> logger,
        IBackOfficeTwoFactorOptions backOfficeTwoFactorOptions,
        IUserTwoFactorLoginService userTwoFactorLoginService,
        IBackOfficeExternalLoginService externalLoginService,
        IBackOfficeUserClientCredentialsManager backOfficeUserClientCredentialsManager)
    {
        _httpContextAccessor = httpContextAccessor;
        _backOfficeSignInManager = backOfficeSignInManager;
        _backOfficeUserManager = backOfficeUserManager;
        _securitySettings = securitySettings;
        _logger = logger;
        _backOfficeTwoFactorOptions = backOfficeTwoFactorOptions;
        _userTwoFactorLoginService = userTwoFactorLoginService;
        _externalLoginService = externalLoginService;
        _backOfficeUserClientCredentialsManager = backOfficeUserClientCredentialsManager;
    }

    [HttpPost("login")]
    [MapToApiVersion("1.0")]
    [Authorize(Policy = AuthorizationPolicies.DenyLocalLoginIfConfigured)]
    public async Task<IActionResult> Login(CancellationToken cancellationToken, LoginRequestModel model)
    {
        // Start a timed scope to ensure failed responses return is a consistent time
        var loginDuration = Math.Max(_loginDurationAverage ?? _securitySettings.Value.UserDefaultFailedLoginDurationInMilliseconds, _securitySettings.Value.UserMinimumFailedLoginDurationInMilliseconds);
        await using var timedScope = new TimedScope(loginDuration, cancellationToken);

        IdentitySignInResult result = await _backOfficeSignInManager.PasswordSignInAsync(model.Username, model.Password, true, true);
        if (result.Succeeded is false)
        {
            // TODO: The result should include the user and whether the credentials were valid to avoid these additional checks
            BackOfficeIdentityUser? user = await _backOfficeUserManager.FindByNameAsync(model.Username.Trim()); // Align with UmbracoSignInManager and trim username!
            if (user is not null &&
                await _backOfficeUserManager.CheckPasswordAsync(user, model.Password))
            {
                // The credentials were correct, so cancel timed scope and provide a more detailed failure response
                await timedScope.CancelAsync();

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

                if (result.RequiresTwoFactor)
                {
                    string? twofactorView = _backOfficeTwoFactorOptions.GetTwoFactorView(model.Username);
                    IEnumerable<string> enabledProviders = (await _userTwoFactorLoginService.GetProviderNamesAsync(user.Key)).Result.Where(x => x.IsEnabledOnUser).Select(x => x.ProviderName);

                    return StatusCode(StatusCodes.Status402PaymentRequired, new RequiresTwoFactorResponseModel()
                    {
                        TwoFactorLoginView = twofactorView,
                        EnabledTwoFactorProviderNames = enabledProviders
                    });
                }
            }

            return StatusCode(StatusCodes.Status401Unauthorized, new ProblemDetailsBuilder()
                .WithTitle("Invalid credentials")
                .WithDetail("The provided credentials are invalid. User has not been signed in.")
                .Build());
        }

        // Set initial or update average (successful) login duration
        _loginDurationAverage = _loginDurationAverage is long average
            ? (average + (long)timedScope.Elapsed.TotalMilliseconds) / 2
            : (long)timedScope.Elapsed.TotalMilliseconds;

        // Cancel the timed scope (we don't want to unnecessarily wait on a successful response)
        await timedScope.CancelAsync();

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
            return BadRequest(new OpenIddictResponse
            {
                Error = "No context found",
                ErrorDescription = "Unable to obtain context from the current request."
            });
        }

        // make sure we keep member authentication away from this endpoint
        if (request.ClientId is Constants.OAuthClientIds.Member)
        {
            return BadRequest(new OpenIddictResponse
            {
                Error = "Invalid 'client ID'",
                ErrorDescription = "The specified 'client_id' is not valid."
            });
        }

        return request.IdentityProvider.IsNullOrWhiteSpace()
            ? await AuthorizeInternal(request)
            : await AuthorizeExternal(request);
    }

    [AllowAnonymous]
    [HttpPost("token")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Token()
    {
        HttpContext context = _httpContextAccessor.GetRequiredHttpContext();
        OpenIddictRequest? request = context.GetOpenIddictServerRequest();
        if (request == null)
        {
            return BadRequest(new OpenIddictResponse
            {
                Error = "No context found",
                ErrorDescription = "Unable to obtain context from the current request."
            });
        }

        if (request.IsAuthorizationCodeGrantType() || request.IsRefreshTokenGrantType())
        {
            // attempt to authorize against the supplied the authorization code
            AuthenticateResult authenticateResult = await context.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            return authenticateResult is { Succeeded: true, Principal: not null }
                ? new SignInResult(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme, authenticateResult.Principal)
                : BadRequest(new OpenIddictResponse
                {
                    Error = "Authorization failed",
                    ErrorDescription = "The supplied authorization could not be verified."
                });
        }

        // ensure the client ID and secret are valid (verified by OpenIddict)
        if (!request.IsClientCredentialsGrantType())
        {
            throw new InvalidOperationException("The requested grant type is not supported.");
        }

        // grab the user associated with the client ID
        BackOfficeIdentityUser? associatedUser = await _backOfficeUserClientCredentialsManager.FindUserAsync(request.ClientId!);
        if (associatedUser is not null)
        {
            // log current datetime as last login (this also ensures that the user is not flagged as inactive)
            associatedUser.LastLoginDateUtc = DateTime.UtcNow;
            await _backOfficeUserManager.UpdateAsync(associatedUser);

            return await SignInBackOfficeUser(associatedUser, request);
        }

        // if this happens, the OpenIddict applications have somehow gone out of sync with the Umbraco users
        _logger.LogError("The user associated with the client ID ({clientId}) could not be found", request.ClientId);

        return BadRequest(new OpenIddictResponse
        {
            Error = "Authorization failed",
            ErrorDescription = "The user associated with the supplied 'client_id' could not be found."
        });
    }

    [AllowAnonymous]
    [HttpGet("signout")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Signout(CancellationToken cancellationToken)
    {
        AuthenticateResult cookieAuthResult = await HttpContext.AuthenticateAsync(Constants.Security.BackOfficeAuthenticationType);
        var userName = cookieAuthResult.Principal?.Identity?.Name;
        var userId = cookieAuthResult.Principal?.Identity?.GetUserId();

        await _backOfficeSignInManager.SignOutAsync();
        _backOfficeUserManager.NotifyLogoutSuccess(cookieAuthResult.Principal ?? User, userId);

        _logger.LogInformation(
            "User {UserName} from IP address {RemoteIpAddress} has logged out",
            userName ?? "UNKNOWN",
            HttpContext.Connection.RemoteIpAddress);

        // Returning a SignOutResult will ask OpenIddict to redirect the user agent
        // to the post_logout_redirect_uri specified by the client application.
        return SignOut(Constants.Security.BackOfficeAuthenticationType, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    // Creates and retains a short lived secret to use in the link-login
    // endpoint because we can not protect that method with a bearer token for reasons explained there
    [HttpGet("link-login-key")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> LinkLoginKey(string provider)
    {
        Attempt<Guid?, ExternalLoginOperationStatus> generateSecretAttempt = await _externalLoginService.GenerateLoginProviderSecretAsync(User, provider);
        return generateSecretAttempt.Success
            ? Ok(generateSecretAttempt.Result)
            : generateSecretAttempt.Status is ExternalLoginOperationStatus.AuthenticationSchemeNotFound
                ? StatusCode(StatusCodes.Status400BadRequest, new ProblemDetailsBuilder()
                .WithTitle("Invalid provider")
                .WithDetail($"No provider with scheme name '{provider}' is configured")
                .Build())
                : Unauthorized();
    }

    /// <summary>
    ///     Called when a user links an external login provider in the back office
    /// </summary>
    /// <param name="requestModel"></param>
    /// <returns></returns>
    // This method is marked as AllowAnonymous and protected with a secret (linkKey) inside the model for the following reasons
    // - when a js client uses the fetch api (or old ajax requests) they can send a bearer token
    //   but since this method returns a redirect (after middleware intervenes) to another domain
    //   and the redirect can not be intercepted, a cors error is thrown on the client
    // - if we switch this method to a form post or a plain get, cors is not an issue, but the client
    //   can't set a bearer token header.
    // we are forcing form usage here for the whole model so the secret does not end up in url logs.
    [HttpPost("link-login")]
    [AllowAnonymous]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> LinkLogin([FromForm] LinkLoginRequestModel requestModel)
    {
        Attempt<ClaimsPrincipal?, ExternalLoginOperationStatus> claimsPrincipleAttempt = await _externalLoginService.ClaimsPrincipleFromLoginProviderLinkKeyAsync(requestModel.Provider, requestModel.LinkKey);

        if (claimsPrincipleAttempt.Success == false)
        {
            return Redirect(_securitySettings.Value.BackOfficeHost + "/" + _securitySettings.Value.AuthorizeCallbackErrorPathName.TrimStart('/').AppendQueryStringToUrl(
                $"{RedirectFlowParameter}=link-login",
                $"{RedirectStatusParameter}=unauthorized"));
        }

        BackOfficeIdentityUser? user = await _backOfficeUserManager.GetUserAsync(claimsPrincipleAttempt.Result!);
        if (user == null)
        {
            return Redirect(_securitySettings.Value.BackOfficeHost + "/" + _securitySettings.Value.AuthorizeCallbackErrorPathName.TrimStart('/').AppendQueryStringToUrl(
                $"{RedirectFlowParameter}=link-login",
                $"{RedirectStatusParameter}=user-not-found"));
        }

        // Request a redirect to the external login provider to link a login for the current user
        var redirectUrl = Url.Action(nameof(ExternalLinkLoginCallback), this.GetControllerName());

        // Configures the redirect URL and user identifier for the specified external login including xsrf data
        AuthenticationProperties properties =
            _backOfficeSignInManager.ConfigureExternalAuthenticationProperties(requestModel.Provider, redirectUrl, user.Id);

        return Challenge(properties, requestModel.Provider);
    }

    /// <summary>
    ///     Callback path when the user initiates a link login request from the back office to the external provider from the
    ///     <see cref="LinkLogin(LinkLoginRequestModel)" /> action
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
        Attempt<IEnumerable<IdentityError>, ExternalLoginOperationStatus> handleResult = await _externalLoginService.HandleLoginCallbackAsync(HttpContext);

        if (handleResult.Success)
        {
            return Redirect(_securitySettings.Value.BackOfficeHost?.GetLeftPart(UriPartial.Authority) ?? Constants.System.DefaultUmbracoPath);
        }

        return handleResult.Status switch
        {
            ExternalLoginOperationStatus.Unauthorized => RedirectWithStatus("unauthorized"),
            ExternalLoginOperationStatus.UserNotFound => RedirectWithStatus("user-not-found"),
            ExternalLoginOperationStatus.ExternalInfoNotFound => RedirectWithStatus("external-info-not-found"),
            ExternalLoginOperationStatus.IdentityFailure => RedirectWithStatus("failed"),
            _ => RedirectWithStatus("unknown-failure")
        };

        RedirectResult RedirectWithStatus(string status)
            => CallbackErrorRedirectWithStatus("external-login-callback", status, handleResult.Result);
    }

    [HttpPost("unlink-login")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> PostUnLinkLogin(UnLinkLoginRequestModel unlinkLoginRequestModel)
    {
        Attempt<ExternalLoginOperationStatus> unlinkResult = await _externalLoginService.UnLinkLoginAsync(
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
                .WithDetail("Could not find the authentication scheme for the supplied provider")
                .Build()),
            ExternalLoginOperationStatus.AuthenticationOptionsNotFound => BadRequest(problemDetailsBuilder
                .WithTitle("Missing Authentication options")
                .WithDetail("Could not find external authentication options for the supplied provider")
                .Build()),
            ExternalLoginOperationStatus.UnlinkingDisabled => BadRequest(problemDetailsBuilder
                .WithTitle("Unlinking disabled")
                .WithDetail("Manual linking is disabled for the supplied provider")
                .Build()),
            ExternalLoginOperationStatus.InvalidProviderKey => BadRequest(problemDetailsBuilder
                .WithTitle("Unlinking failed")
                .WithDetail("Could not match ProviderKey to the supplied provider")
                .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, "Unknown external login operation status."),
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

                // sign in the backoffice user from the HttpContext, as thas was set doing the ExternalLoginSignInAsync
                ClaimsPrincipal backOfficePrincipal = HttpContext.User;
                return await SignInBackOfficeUser(backOfficePrincipal, request);
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

    private Task<IActionResult> SignInBackOfficeUser(ClaimsPrincipal backOfficePrincipal, OpenIddictRequest request)
    {
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

        return Task.FromResult<IActionResult>(new SignInResult(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme, backOfficePrincipal));
    }

    private async Task<IActionResult> SignInBackOfficeUser(BackOfficeIdentityUser backOfficeUser, OpenIddictRequest request)
    {
        ClaimsPrincipal backOfficePrincipal = await _backOfficeSignInManager.CreateUserPrincipalAsync(backOfficeUser);

        return await SignInBackOfficeUser(backOfficePrincipal, request);
    }

    private static IActionResult DefaultChallengeResult() => new ChallengeResult(Constants.Security.BackOfficeAuthenticationType);

    private RedirectResult CallbackErrorRedirectWithStatus(string flowType, string status, IEnumerable<IdentityError> identityErrors)
    {
        var redirectUrl = _securitySettings.Value.BackOfficeHost + "/" +
                          _securitySettings.Value.AuthorizeCallbackErrorPathName.TrimStart('/').AppendQueryStringToUrl(
                              $"{RedirectFlowParameter}={flowType}",
                              $"{RedirectStatusParameter}={status}");
        foreach (IdentityError identityError in identityErrors)
        {
            redirectUrl.AppendQueryStringToUrl($"{RedirectErrorCodeParameter}={identityError.Code}");
        }

        return Redirect(redirectUrl);
    }
}
