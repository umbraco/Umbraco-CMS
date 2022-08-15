using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Mail;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Email;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Net;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Security;
using Umbraco.Cms.Web.BackOffice.Filters;
using Umbraco.Cms.Web.BackOffice.Security;
using Umbraco.Cms.Web.Common.ActionsResults;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Web.Common.Filters;
using Umbraco.Cms.Web.Common.Models;
using Umbraco.Extensions;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace Umbraco.Cms.Web.BackOffice.Controllers;
// See
// for a bigger example of this type of controller implementation in netcore:
// https://github.com/dotnet/AspNetCore.Docs/blob/2efb4554f8f659be97ee7cd5dd6143b871b330a5/aspnetcore/migration/1x-to-2x/samples/AspNetCoreDotNetCore2App/AspNetCoreDotNetCore2App/Controllers/AccountController.cs
// https://github.com/dotnet/AspNetCore.Docs/blob/ad16f5e1da6c04fa4996ee67b513f2a90fa0d712/aspnetcore/common/samples/WebApplication1/Controllers/AccountController.cs
// with authenticator app
// https://github.com/dotnet/AspNetCore.Docs/blob/master/aspnetcore/security/authentication/identity/sample/src/ASPNETCore-IdentityDemoComplete/IdentityDemo/Controllers/AccountController.cs

[PluginController(Constants.Web.Mvc
    .BackOfficeApiArea)] // TODO: Maybe this could be applied with our Application Model conventions
//[ValidationFilter] // TODO: I don't actually think this is required with our custom Application Model conventions applied
[AngularJsonOnlyConfiguration] // TODO: This could be applied with our Application Model conventions
[IsBackOffice]
[DisableBrowserCache]
public class AuthenticationController : UmbracoApiControllerBase
{
    // NOTE: Each action must either be explicitly authorized or explicitly [AllowAnonymous], the latter is optional because
    // this controller itself doesn't require authz but it's more clear what the intention is.

    private readonly IBackOfficeSecurityAccessor _backofficeSecurityAccessor;
    private readonly IBackOfficeTwoFactorOptions _backOfficeTwoFactorOptions;
    private readonly IEmailSender _emailSender;
    private readonly IBackOfficeExternalLoginProviders _externalAuthenticationOptions;
    private readonly GlobalSettings _globalSettings;
    private readonly IHostingEnvironment _hostingEnvironment;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IIpResolver _ipResolver;
    private readonly LinkGenerator _linkGenerator;
    private readonly ILogger<AuthenticationController> _logger;
    private readonly UserPasswordConfigurationSettings _passwordConfiguration;
    private readonly SecuritySettings _securitySettings;
    private readonly IBackOfficeSignInManager _signInManager;
    private readonly ISmsSender _smsSender;
    private readonly ILocalizedTextService _textService;
    private readonly ITwoFactorLoginService _twoFactorLoginService;
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly IBackOfficeUserManager _userManager;
    private readonly IUserService _userService;
    private readonly WebRoutingSettings _webRoutingSettings;

    // TODO: We need to review all _userManager.Raise calls since many/most should be on the usermanager or signinmanager, very few should be here
    [ActivatorUtilitiesConstructor]
    public AuthenticationController(
        IBackOfficeSecurityAccessor backofficeSecurityAccessor,
        IBackOfficeUserManager backOfficeUserManager,
        IBackOfficeSignInManager signInManager,
        IUserService userService,
        ILocalizedTextService textService,
        IUmbracoMapper umbracoMapper,
        IOptionsSnapshot<GlobalSettings> globalSettings,
        IOptionsSnapshot<SecuritySettings> securitySettings,
        ILogger<AuthenticationController> logger,
        IIpResolver ipResolver,
        IOptionsSnapshot<UserPasswordConfigurationSettings> passwordConfiguration,
        IEmailSender emailSender,
        ISmsSender smsSender,
        IHostingEnvironment hostingEnvironment,
        LinkGenerator linkGenerator,
        IBackOfficeExternalLoginProviders externalAuthenticationOptions,
        IBackOfficeTwoFactorOptions backOfficeTwoFactorOptions,
        IHttpContextAccessor httpContextAccessor,
        IOptions<WebRoutingSettings> webRoutingSettings,
        ITwoFactorLoginService twoFactorLoginService)
    {
        _backofficeSecurityAccessor = backofficeSecurityAccessor;
        _userManager = backOfficeUserManager;
        _signInManager = signInManager;
        _userService = userService;
        _textService = textService;
        _umbracoMapper = umbracoMapper;
        _globalSettings = globalSettings.Value;
        _securitySettings = securitySettings.Value;
        _logger = logger;
        _ipResolver = ipResolver;
        _passwordConfiguration = passwordConfiguration.Value;
        _emailSender = emailSender;
        _smsSender = smsSender;
        _hostingEnvironment = hostingEnvironment;
        _linkGenerator = linkGenerator;
        _externalAuthenticationOptions = externalAuthenticationOptions;
        _backOfficeTwoFactorOptions = backOfficeTwoFactorOptions;
        _httpContextAccessor = httpContextAccessor;
        _webRoutingSettings = webRoutingSettings.Value;
        _twoFactorLoginService = twoFactorLoginService;
    }

    /// <summary>
    ///     Returns the configuration for the backoffice user membership provider - used to configure the change password
    ///     dialog
    /// </summary>
    [AllowAnonymous] // Needed for users that are invited when they use the link from the mail they are not authorized
    [Authorize(Policy =
        AuthorizationPolicies.BackOfficeAccess)] // Needed to enforce the principle set on the request, if one exists.
    public IDictionary<string, object> GetPasswordConfig(int userId)
    {
        Attempt<int> currentUserId =
            _backofficeSecurityAccessor.BackOfficeSecurity?.GetUserId() ?? Attempt<int>.Fail();
        return _passwordConfiguration.GetConfiguration(
            currentUserId.Success
                ? currentUserId.Result != userId
                : true);
    }

    /// <summary>
    ///     Checks if a valid token is specified for an invited user and if so logs the user in and returns the user object
    /// </summary>
    /// <param name="id"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    /// <remarks>
    ///     This will also update the security stamp for the user so it can only be used once
    /// </remarks>
    [ValidateAngularAntiForgeryToken]
    [Authorize(Policy = AuthorizationPolicies.DenyLocalLoginIfConfigured)]
    public async Task<ActionResult<UserDisplay?>> PostVerifyInvite([FromQuery] int id, [FromQuery] string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return NotFound();
        }

        var decoded = token.FromUrlBase64();
        if (decoded.IsNullOrWhiteSpace())
        {
            return NotFound();
        }

        BackOfficeIdentityUser? identityUser = await _userManager.FindByIdAsync(id.ToString());
        if (identityUser == null)
        {
            return NotFound();
        }

        IdentityResult result = await _userManager.ConfirmEmailAsync(identityUser, decoded!);

        if (result.Succeeded == false)
        {
            return ValidationErrorResult.CreateNotificationValidationErrorResult(result.Errors.ToErrorMessage());
        }

        await _signInManager.SignOutAsync();

        await _signInManager.SignInAsync(identityUser, false);

        IUser? user = _userService.GetUserById(id);

        return _umbracoMapper.Map<UserDisplay>(user);
    }

    [Authorize(Policy = AuthorizationPolicies.BackOfficeAccess)]
    [ValidateAngularAntiForgeryToken]
    public async Task<IActionResult> PostUnLinkLogin(UnLinkLoginModel unlinkLoginModel)
    {
        BackOfficeIdentityUser? user = await _userManager.FindByIdAsync(User.Identity?.GetUserId());
        if (user == null)
        {
            throw new InvalidOperationException("Could not find user");
        }

        AuthenticationScheme? authType = (await _signInManager.GetExternalAuthenticationSchemesAsync())
            .FirstOrDefault(x => x.Name == unlinkLoginModel.LoginProvider);

        if (authType == null)
        {
            _logger.LogWarning("Could not find external authentication provider registered: {LoginProvider}", unlinkLoginModel.LoginProvider);
        }
        else
        {
            BackOfficeExternaLoginProviderScheme? opt = await _externalAuthenticationOptions.GetAsync(authType.Name);
            if (opt == null)
            {
                return BadRequest(
                    $"Could not find external authentication options registered for provider {unlinkLoginModel.LoginProvider}");
            }

            if (!opt.ExternalLoginProvider.Options.AutoLinkOptions.AllowManualLinking)
            {
                // If AllowManualLinking is disabled for this provider we cannot unlink
                return BadRequest();
            }
        }

        IdentityResult result = await _userManager.RemoveLoginAsync(
            user,
            unlinkLoginModel.LoginProvider,
            unlinkLoginModel.ProviderKey);

        if (result.Succeeded)
        {
            await _signInManager.SignInAsync(user, true);
            return Ok();
        }

        AddModelErrors(result);
        return new ValidationErrorResult(ModelState);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<double> GetRemainingTimeoutSeconds()
    {
        // force authentication to occur since this is not an authorized endpoint
        AuthenticateResult result = await this.AuthenticateBackOfficeAsync();
        if (!result.Succeeded)
        {
            return 0;
        }

        var remainingSeconds = result.Principal.GetRemainingAuthSeconds();
        if (remainingSeconds <= 30)
        {
            var username = result.Principal.FindFirst(ClaimTypes.Name)?.Value;

            //NOTE: We are using 30 seconds because that is what is coded into angular to force logout to give some headway in
            // the timeout process.

            _logger.LogInformation(
                "User logged will be logged out due to timeout: {Username}, IP Address: {IPAddress}",
                username ?? "unknown",
                _ipResolver.GetCurrentRequestIpAddress());
        }

        return remainingSeconds;
    }

    /// <summary>
    ///     Checks if the current user's cookie is valid and if so returns OK or a 400 (BadRequest)
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [AllowAnonymous]
    public async Task<bool> IsAuthenticated()
    {
        // force authentication to occur since this is not an authorized endpoint
        AuthenticateResult result = await this.AuthenticateBackOfficeAsync();
        return result.Succeeded;
    }

    /// <summary>
    ///     Returns the currently logged in Umbraco user
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    ///     We have the attribute [SetAngularAntiForgeryTokens] applied because this method is called initially to determine if
    ///     the user
    ///     is valid before the login screen is displayed. The Auth cookie can be persisted for up to a day but the csrf
    ///     cookies are only session
    ///     cookies which means that the auth cookie could be valid but the csrf cookies are no longer there, in that case we
    ///     need to re-set the csrf cookies.
    /// </remarks>
    [Authorize(Policy = AuthorizationPolicies.BackOfficeAccess)]
    [SetAngularAntiForgeryTokens]
    [CheckIfUserTicketDataIsStale]
    public UserDetail? GetCurrentUser()
    {
        IUser? user = _backofficeSecurityAccessor.BackOfficeSecurity?.CurrentUser;
        UserDetail? result = _umbracoMapper.Map<UserDetail>(user);

        if (result is not null)
        {
            //set their remaining seconds
            result.SecondsUntilTimeout = HttpContext.User.GetRemainingAuthSeconds();
        }

        return result;
    }

    /// <summary>
    ///     When a user is invited they are not approved but we need to resolve the partially logged on (non approved)
    ///     user.
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    ///     We cannot user GetCurrentUser since that requires they are approved, this is the same as GetCurrentUser but doesn't
    ///     require them to be approved
    /// </remarks>
    [Authorize(Policy = AuthorizationPolicies.BackOfficeAccessWithoutApproval)]
    [SetAngularAntiForgeryTokens]
    [Authorize(Policy = AuthorizationPolicies.DenyLocalLoginIfConfigured)]
    public ActionResult<UserDetail?> GetCurrentInvitedUser()
    {
        IUser? user = _backofficeSecurityAccessor.BackOfficeSecurity?.CurrentUser;

        if (user?.IsApproved ?? false)
        {
            // if they are approved, than they are no longer invited and we can return an error
            return Forbid();
        }

        UserDetail? result = _umbracoMapper.Map<UserDetail>(user);

        if (result is not null)
        {
            // set their remaining seconds
            result.SecondsUntilTimeout = HttpContext.User.GetRemainingAuthSeconds();
        }

        return result;
    }

    /// <summary>
    ///     Logs a user in
    /// </summary>
    /// <returns></returns>
    [SetAngularAntiForgeryTokens]
    [Authorize(Policy = AuthorizationPolicies.DenyLocalLoginIfConfigured)]
    public async Task<ActionResult<UserDetail?>> PostLogin(LoginModel loginModel)
    {
        // Sign the user in with username/password, this also gives a chance for developers to
        // custom verify the credentials and auto-link user accounts with a custom IBackOfficePasswordChecker
        SignInResult result = await _signInManager.PasswordSignInAsync(
            loginModel.Username, loginModel.Password, true, true);

        if (result.Succeeded)
        {
            // return the user detail
            return GetUserDetail(_userService.GetByUsername(loginModel.Username));
        }

        if (result.RequiresTwoFactor)
        {
            var twofactorView = _backOfficeTwoFactorOptions.GetTwoFactorView(loginModel.Username);
            if (twofactorView.IsNullOrWhiteSpace())
            {
                return new ValidationErrorResult(
                    $"The registered {typeof(IBackOfficeTwoFactorOptions)} of type {_backOfficeTwoFactorOptions.GetType()} did not return a view for two factor auth ");
            }

            IUser? attemptedUser = _userService.GetByUsername(loginModel.Username);

            // create a with information to display a custom two factor send code view
            var verifyResponse =
                new ObjectResult(new { twoFactorView = twofactorView, userId = attemptedUser?.Id })
                {
                    StatusCode = StatusCodes.Status402PaymentRequired
                };

            return verifyResponse;
        }

        // TODO: We can check for these and respond differently if we think it's important
        //  result.IsLockedOut
        //  result.IsNotAllowed

        // return BadRequest (400), we don't want to return a 401 because that get's intercepted
        // by our angular helper because it thinks that we need to re-perform the request once we are
        // authorized and we don't want to return a 403 because angular will show a warning message indicating
        // that the user doesn't have access to perform this function, we just want to return a normal invalid message.
        return BadRequest();
    }

    /// <summary>
    ///     Processes a password reset request.  Looks for a match on the provided email address
    ///     and if found sends an email with a link to reset it
    /// </summary>
    /// <returns></returns>
    [SetAngularAntiForgeryTokens]
    [Authorize(Policy = AuthorizationPolicies.DenyLocalLoginIfConfigured)]
    public async Task<IActionResult> PostRequestPasswordReset(RequestPasswordResetModel model)
    {
        // If this feature is switched off in configuration the UI will be amended to not make the request to reset password available.
        // So this is just a server-side secondary check.
        if (_securitySettings.AllowPasswordReset == false)
        {
            return BadRequest();
        }

        BackOfficeIdentityUser? identityUser = await _userManager.FindByEmailAsync(model.Email);
        if (identityUser != null)
        {
            IUser? user = _userService.GetByEmail(model.Email);
            if (user != null)
            {
                var from = _globalSettings.Smtp?.From;
                var code = await _userManager.GeneratePasswordResetTokenAsync(identityUser);
                var callbackUrl = ConstructCallbackUrl(identityUser.Id, code);

                var message = _textService.Localize("login", "resetPasswordEmailCopyFormat",
                    // Ensure the culture of the found user is used for the email!
                    UmbracoUserExtensions.GetUserCulture(identityUser.Culture, _textService, _globalSettings),
                    new[] { identityUser.UserName, callbackUrl });

                var subject = _textService.Localize("login", "resetPasswordEmailCopySubject",
                    // Ensure the culture of the found user is used for the email!
                    UmbracoUserExtensions.GetUserCulture(identityUser.Culture, _textService, _globalSettings));

                var mailMessage = new EmailMessage(from, user.Email, subject, message, true);

                await _emailSender.SendAsync(mailMessage, Constants.Web.EmailTypes.PasswordReset);

                _userManager.NotifyForgotPasswordRequested(User, user.Id.ToString());
            }
        }

        return Ok();
    }

    /// <summary>
    ///     Used to retrieve the 2FA providers for code submission
    /// </summary>
    /// <returns></returns>
    [SetAngularAntiForgeryTokens]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<string>>> Get2FAProviders()
    {
        BackOfficeIdentityUser? user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
        if (user == null)
        {
            _logger.LogWarning("Get2FAProviders :: No verified user found, returning 404");
            return NotFound();
        }

        IEnumerable<string> userFactors = await _twoFactorLoginService.GetEnabledTwoFactorProviderNamesAsync(user.Key);

        return new ObjectResult(userFactors);
    }

    [SetAngularAntiForgeryTokens]
    [AllowAnonymous]
    public async Task<IActionResult> PostSend2FACode([FromBody] string provider)
    {
        if (provider.IsNullOrWhiteSpace())
        {
            return NotFound();
        }

        BackOfficeIdentityUser? user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
        if (user == null)
        {
            _logger.LogWarning("PostSend2FACode :: No verified user found, returning 404");
            return NotFound();
        }

        var from = _globalSettings.Smtp?.From;
        // Generate the token and send it
        var code = await _userManager.GenerateTwoFactorTokenAsync(user, provider);
        if (string.IsNullOrWhiteSpace(code))
        {
            _logger.LogWarning("PostSend2FACode :: Could not generate 2FA code");
            return BadRequest("Invalid code");
        }

        var subject = _textService.Localize("login", "mfaSecurityCodeSubject",
            // Ensure the culture of the found user is used for the email!
            UmbracoUserExtensions.GetUserCulture(user.Culture, _textService, _globalSettings));

        var message = _textService.Localize("login", "mfaSecurityCodeMessage",
            // Ensure the culture of the found user is used for the email!
            UmbracoUserExtensions.GetUserCulture(user.Culture, _textService, _globalSettings),
            new[] { code });

        if (provider == "Email")
        {
            var mailMessage = new EmailMessage(from, user.Email, subject, message, true);

            await _emailSender.SendAsync(mailMessage, Constants.Web.EmailTypes.TwoFactorAuth);
        }
        else if (provider == "Phone")
        {
            await _smsSender.SendSmsAsync(await _userManager.GetPhoneNumberAsync(user), message);
        }

        return Ok();
    }

    [SetAngularAntiForgeryTokens]
    [AllowAnonymous]
    public async Task<ActionResult<UserDetail?>> PostVerify2FACode(Verify2FACodeModel model)
    {
        if (ModelState.IsValid == false)
        {
            return new ValidationErrorResult(ModelState);
        }

        BackOfficeIdentityUser? user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
        if (user == null)
        {
            _logger.LogWarning("PostVerify2FACode :: No verified user found, returning 404");
            return NotFound();
        }

        SignInResult result =
            await _signInManager.TwoFactorSignInAsync(model.Provider, model.Code, model.IsPersistent, model.RememberClient);
        if (result.Succeeded)
        {
            return GetUserDetail(_userService.GetByUsername(user.UserName));
        }

        if (result.IsLockedOut)
        {
            return new ValidationErrorResult("User is locked out");
        }

        if (result.IsNotAllowed)
        {
            return new ValidationErrorResult("User is not allowed");
        }

        return new ValidationErrorResult("Invalid code");
    }

    /// <summary>
    ///     Processes a set password request.  Validates the request and sets a new password.
    /// </summary>
    /// <returns></returns>
    [SetAngularAntiForgeryTokens]
    [AllowAnonymous]
    public async Task<IActionResult> PostSetPassword(SetPasswordModel model)
    {
        BackOfficeIdentityUser? identityUser =
            await _userManager.FindByIdAsync(model.UserId.ToString(CultureInfo.InvariantCulture));

        IdentityResult result = await _userManager.ResetPasswordAsync(identityUser, model.ResetCode, model.Password);
        if (result.Succeeded)
        {
            var lockedOut = await _userManager.IsLockedOutAsync(identityUser);
            if (lockedOut)
            {
                _logger.LogInformation(
                    "User {UserId} is currently locked out, unlocking and resetting AccessFailedCount", model.UserId);

                //// var user = await UserManager.FindByIdAsync(model.UserId);
                IdentityResult unlockResult =
                    await _userManager.SetLockoutEndDateAsync(identityUser, DateTimeOffset.Now);
                if (unlockResult.Succeeded == false)
                {
                    _logger.LogWarning("Could not unlock for user {UserId} - error {UnlockError}", model.UserId,
                        unlockResult.Errors.First().Description);
                }

                IdentityResult resetAccessFailedCountResult =
                    await _userManager.ResetAccessFailedCountAsync(identityUser);
                if (resetAccessFailedCountResult.Succeeded == false)
                {
                    _logger.LogWarning("Could not reset access failed count {UserId} - error {UnlockError}",
                        model.UserId, unlockResult.Errors.First().Description);
                }
            }

            // They've successfully set their password, we can now update their user account to be confirmed
            // if user was only invited, then they have not been approved
            // but a successful forgot password flow (e.g. if their token had expired and they did a forgot password instead of request new invite)
            // means we have verified their email
            if (!await _userManager.IsEmailConfirmedAsync(identityUser))
            {
                await _userManager.ConfirmEmailAsync(identityUser, model.ResetCode);
            }

            // invited is not approved, never logged in, invited date present
            /*
            if (LastLoginDate == default && IsApproved == false && InvitedDate != null)
                return UserState.Invited;
            */
            if (identityUser != null && !identityUser.IsApproved)
            {
                IUser? user = _userService.GetByUsername(identityUser.UserName);
                // also check InvitedDate and never logged in, otherwise this would allow a disabled user to reactivate their account with a forgot password
                if (user?.LastLoginDate == default && user?.InvitedDate != null)
                {
                    user.IsApproved = true;
                    user.InvitedDate = null;
                    _userService.Save(user);
                }
            }

            _userManager.NotifyForgotPasswordChanged(User, model.UserId.ToString(CultureInfo.InvariantCulture));
            return Ok();
        }

        return new ValidationErrorResult(
            result.Errors.Any() ? result.Errors.First().Description : "Set password failed");
    }

    /// <summary>
    ///     Logs the current user out
    /// </summary>
    /// <returns></returns>
    [ValidateAngularAntiForgeryToken]
    [AllowAnonymous]
    public async Task<IActionResult> PostLogout()
    {
        // force authentication to occur since this is not an authorized endpoint
        AuthenticateResult result = await this.AuthenticateBackOfficeAsync();
        if (!result.Succeeded)
        {
            return Ok();
        }

        await _signInManager.SignOutAsync();

        _logger.LogInformation("User {UserName} from IP address {RemoteIpAddress} has logged out",
            User.Identity == null ? "UNKNOWN" : User.Identity.Name, HttpContext.Connection.RemoteIpAddress);

        var userId = result.Principal.Identity?.GetUserId();
        SignOutSuccessResult args = _userManager.NotifyLogoutSuccess(User, userId);
        if (!args.SignOutRedirectUrl.IsNullOrWhiteSpace())
        {
            return new ObjectResult(new { signOutRedirectUrl = args.SignOutRedirectUrl });
        }

        return Ok();
    }


    /// <summary>
    ///     Return the <see cref="UserDetail" /> for the given <see cref="IUser" />
    /// </summary>
    /// <param name="user"></param>
    /// <param name="principal"></param>
    /// <returns></returns>
    private UserDetail? GetUserDetail(IUser? user)
    {
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        UserDetail? userDetail = _umbracoMapper.Map<UserDetail>(user);

        if (userDetail is not null)
        {
            // update the userDetail and set their remaining seconds
            userDetail.SecondsUntilTimeout = _globalSettings.TimeOut.TotalSeconds;
        }

        return userDetail;
    }

    private string ConstructCallbackUrl(string userId, string code)
    {
        // Get an mvc helper to get the url
        var action = _linkGenerator.GetPathByAction(
            nameof(BackOfficeController.ValidatePasswordResetCode),
            ControllerExtensions.GetControllerName<BackOfficeController>(),
            new { area = Constants.Web.Mvc.BackOfficeArea, u = userId, r = code });

        // Construct full URL using configured application URL (which will fall back to current request)
        Uri applicationUri = _httpContextAccessor.GetRequiredHttpContext().Request
            .GetApplicationUri(_webRoutingSettings);
        var callbackUri = new Uri(applicationUri, action);
        return callbackUri.ToString();
    }

    private void AddModelErrors(IdentityResult result, string prefix = "")
    {
        foreach (IdentityError? error in result.Errors)
        {
            ModelState.AddModelError(prefix, error.Description);
        }
    }
}
