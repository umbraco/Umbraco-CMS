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

/// <summary>
/// Provides endpoints for managing security-related operations in the back office.
/// </summary>
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

    /// <summary>
    /// Initializes a new instance of the <see cref="BackOfficeController"/> class.
    /// </summary>
    /// <param name="httpContextAccessor">Provides access to the current HTTP context.</param>
    /// <param name="backOfficeSignInManager">Manages sign-in operations for back office users.</param>
    /// <param name="backOfficeUserManager">Manages back office user accounts.</param>
    /// <param name="securitySettings">The security settings for the back office.</param>
    /// <param name="logger">The logger used for logging information and errors.</param>
    /// <param name="backOfficeTwoFactorOptions">Options for back office two-factor authentication.</param>
    /// <param name="userTwoFactorLoginService">Service for handling user two-factor login operations.</param>
    /// <param name="externalLoginService">Service for managing external logins in the back office.</param>
    /// <param name="backOfficeUserClientCredentialsManager">Manages client credentials for back office users.</param>
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

    /// <summary>
    /// Authenticates a backoffice user with the provided credentials and returns the result of the login attempt.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <param name="model">The login request model containing the user's credentials.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> indicating the result of the login attempt:
    /// <list type="bullet">
    /// <item><description>200 OK if authentication succeeds.</description></item>
    /// <item><description>401 Unauthorized if credentials are invalid.</description></item>
    /// <item><description>402 PaymentRequired if two-factor authentication is required.</description></item>
    /// <item><description>403 Forbidden if the user is not allowed or is locked out.</description></item>
    /// </list>
    /// </returns>
    [HttpPost("login")]
    [EndpointSummary("Authenticates a user.")]
    [EndpointDescription("Authenticates a user with the provided credentials and returns authentication tokens.")]
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

    /// <summary>
    /// Verifies the two-factor authentication (2FA) code submitted by the user during the sign-in process.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="model">An instance of <see cref="Verify2FACodeModel"/> containing the 2FA provider, code, and related options.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> indicating the result of the verification:
    /// <list type="bullet">
    /// <item><description><c>200 OK</c> if the code is valid.</description></item>
    /// <item><description><c>400 Bad Request</c> if the code is invalid or the model state is invalid.</description></item>
    /// <item><description><c>401 Unauthorized</c> if no user is found for verification.</description></item>
    /// <item><description><c>403 Forbidden</c> if the user is locked out or not allowed to sign in.</description></item>
    /// </list>
    /// </returns>
    [AllowAnonymous]
    [HttpPost("verify-2fa")]
    [EndpointSummary("Verifies two-factor authentication.")]
    [EndpointDescription("Verifies the two-factor authentication code for the user.")]
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

    /// <summary>
    /// Handles authorization requests for the backoffice by validating and processing OAuth authorization requests.
    /// Returns an error if the request context is invalid or if the client is not permitted.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The result contains an <see cref="IActionResult"/> indicating the outcome of the authorization request, including possible error responses for invalid contexts or unauthorized clients.
    /// </returns>
    [AllowAnonymous]
    [HttpGet("authorize")]
    [EndpointSummary("Authorizes the current request.")]
    [EndpointDescription("Validates and authorizes the OAuth authorization request.")]
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
    /// <summary>
    /// Issues or refreshes access tokens for authenticated users via supported OpenID Connect grant types.
    /// </summary>
    /// <remarks>
    /// Supports authorization code, refresh token, and client credentials grant types. Returns an error if the request is invalid or the user cannot be found.
    /// </remarks>
    /// <returns>An <see cref="IActionResult"/> representing the result of the token issuance or refresh process, including error details if applicable.</returns>

    [AllowAnonymous]
    [HttpPost("token")]
    [EndpointSummary("Issues access tokens.")]
    [EndpointDescription("Issues or refreshes access tokens for authenticated users.")]
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
            associatedUser.LastLoginDate = DateTime.UtcNow;
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

    /// <summary>
    /// Signs out the currently authenticated user and ends their session.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous sign-out operation. The task result contains an <see cref="IActionResult"/> that redirects the user agent to the post logout URI.</returns>
    [AllowAnonymous]
    [HttpGet("signout")]
    [EndpointSummary("Signs out the current user.")]
    [EndpointDescription("Signs out the currently authenticated user and ends their session.")]
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
    /// <summary>
    /// Generates a short-lived secret key for use in the link login endpoint, allowing external login providers to securely link accounts without requiring a bearer token.
    /// </summary>
    /// <param name="provider">The scheme name of the external login provider for which to generate the secret key.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> containing the generated secret key as a <see cref="Guid"/>,
    /// or an error response if the provider is invalid or the operation is unauthorized.
    /// </returns>
    [HttpGet("link-login-key")]
    [EndpointSummary("Generates a link login key.")]
    [EndpointDescription("Generates a short-lived secret key for use in the link login endpoint.")]
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
    [EndpointSummary("Links an external login provider.")]
    [EndpointDescription("Links an external login provider to the current user's account.")]
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
    [EndpointSummary("Handles an external link login callback.")]
    [EndpointDescription("Handles the callback from an external login provider after linking.")]
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

    /// <summary>
    /// Unlinks an external login provider from the current user's account.
    /// </summary>
    /// <param name="unlinkLoginRequestModel">The request model containing the external login provider and provider key to unlink.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> indicating the result of the unlink operation. Returns <c>200 OK</c> if successful, or an appropriate error response if the operation fails (e.g., <c>400 Bad Request</c>, <c>401 Unauthorized</c>, or <c>500 Internal Server Error</c>).
    /// </returns>
    [HttpPost("unlink-login")]
    [EndpointSummary("Unlinks an external login provider.")]
    [EndpointDescription("Unlinks an external login provider from the current user's account.")]
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
