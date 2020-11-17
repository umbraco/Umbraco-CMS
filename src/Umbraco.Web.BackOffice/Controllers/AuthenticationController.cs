﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Umbraco.Core;
using Umbraco.Core.BackOffice;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Mapping;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Extensions;
using Umbraco.Net;
using Umbraco.Web.BackOffice.Filters;
using Umbraco.Web.Common.ActionsResults;
using Umbraco.Web.Common.Attributes;
using Umbraco.Web.Common.Controllers;
using Umbraco.Web.Common.Exceptions;
using Umbraco.Web.Common.Filters;
using Umbraco.Web.Common.Security;
using Umbraco.Web.Models;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Security;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.BackOffice.Controllers
{
    [PluginController(Constants.Web.Mvc.BackOfficeApiArea)]  // TODO: Maybe this could be applied with our Application Model conventions
    //[ValidationFilter] // TODO: I don't actually think this is required with our custom Application Model conventions applied
    [AngularJsonOnlyConfiguration] // TODO: This could be applied with our Application Model conventions
    [IsBackOffice] // TODO: This could be applied with our Application Model conventions
    public class AuthenticationController : UmbracoApiControllerBase
    {
        private readonly IBackofficeSecurityAccessor _backofficeSecurityAccessor;
        private readonly IBackOfficeUserManager _userManager;
        private readonly BackOfficeSignInManager _signInManager;
        private readonly IUserService _userService;
        private readonly ILocalizedTextService _textService;
        private readonly UmbracoMapper _umbracoMapper;
        private readonly GlobalSettings _globalSettings;
        private readonly SecuritySettings _securitySettings;
        private readonly ILogger<AuthenticationController> _logger;
        private readonly IIpResolver _ipResolver;
        private readonly UserPasswordConfigurationSettings _passwordConfiguration;
        private readonly IEmailSender _emailSender;
        private readonly Core.Hosting.IHostingEnvironment _hostingEnvironment;
        private readonly IRequestAccessor _requestAccessor;
        private readonly LinkGenerator _linkGenerator;

        // TODO: We need to import the logic from Umbraco.Web.Editors.AuthenticationController
        // TODO: We need to review all _userManager.Raise calls since many/most should be on the usermanager or signinmanager, very few should be here

        public AuthenticationController(
            IBackofficeSecurityAccessor backofficeSecurityAccessor,
            IBackOfficeUserManager backOfficeUserManager,
            BackOfficeSignInManager signInManager,
            IUserService userService,
            ILocalizedTextService textService,
            UmbracoMapper umbracoMapper,
            IOptions<GlobalSettings> globalSettings,
            IOptions<SecuritySettings> securitySettings,
            ILogger<AuthenticationController> logger,
            IIpResolver ipResolver,
            IOptions<UserPasswordConfigurationSettings> passwordConfiguration,
            IEmailSender emailSender,
            Core.Hosting.IHostingEnvironment hostingEnvironment,
            IRequestAccessor requestAccessor,
            LinkGenerator linkGenerator)
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
            _hostingEnvironment = hostingEnvironment;
            _requestAccessor = requestAccessor;
            _linkGenerator = linkGenerator;
        }

        /// <summary>
        /// Returns the configuration for the backoffice user membership provider - used to configure the change password dialog
        /// </summary>
        /// <returns></returns>
        [UmbracoBackOfficeAuthorize]
        public IDictionary<string, object> GetPasswordConfig(int userId)
        {
            return _passwordConfiguration.GetConfiguration(userId != _backofficeSecurityAccessor.BackofficeSecurity.CurrentUser.Id);
        }

        /// <summary>
        /// Checks if a valid token is specified for an invited user and if so logs the user in and returns the user object
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        /// <remarks>
        /// This will also update the security stamp for the user so it can only be used once
        /// </remarks>
        [ValidateAngularAntiForgeryToken]
        public async Task<ActionResult<UserDisplay>> PostVerifyInvite([FromQuery] int id, [FromQuery] string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return NotFound();

            var decoded = token.FromUrlBase64();
            if (decoded.IsNullOrWhiteSpace())
                return NotFound();

            var identityUser = await _userManager.FindByIdAsync(id.ToString());
            if (identityUser == null)
                return NotFound();

            var result = await _userManager.ConfirmEmailAsync(identityUser, decoded);

            if (result.Succeeded == false)
            {
                throw HttpResponseException.CreateNotificationValidationErrorResponse(result.Errors.ToErrorMessage());
            }

            await _signInManager.SignOutAsync();

            await _signInManager.SignInAsync(identityUser, false);

            var user = _userService.GetUserById(id);

            return _umbracoMapper.Map<UserDisplay>(user);
        }

        [HttpGet]
        public double GetRemainingTimeoutSeconds()
        {
            var backOfficeIdentity = HttpContext.User.GetUmbracoIdentity();
            var remainingSeconds = HttpContext.User.GetRemainingAuthSeconds();
            if (remainingSeconds <= 30 && backOfficeIdentity != null)
            {
                //NOTE: We are using 30 seconds because that is what is coded into angular to force logout to give some headway in
                // the timeout process.

                _logger.LogInformation(
                    "User logged will be logged out due to timeout: {Username}, IP Address: {IPAddress}",
                    backOfficeIdentity.Name,
                    _ipResolver.GetCurrentRequestIpAddress());
            }

            return remainingSeconds;
        }

        /// <summary>
        /// Checks if the current user's cookie is valid and if so returns OK or a 400 (BadRequest)
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public bool IsAuthenticated()
        {
            var attempt = _backofficeSecurityAccessor.BackofficeSecurity.AuthorizeRequest();
            if (attempt == ValidateRequestAttempt.Success)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns the currently logged in Umbraco user
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// We have the attribute [SetAngularAntiForgeryTokens] applied because this method is called initially to determine if the user
        /// is valid before the login screen is displayed. The Auth cookie can be persisted for up to a day but the csrf cookies are only session
        /// cookies which means that the auth cookie could be valid but the csrf cookies are no longer there, in that case we need to re-set the csrf cookies.
        /// </remarks>
        [UmbracoBackOfficeAuthorize]
        [SetAngularAntiForgeryTokens]
        //[CheckIfUserTicketDataIsStale] // TODO: Migrate this, though it will need to be done differently at the cookie auth level
        public UserDetail GetCurrentUser()
        {
            var user = _backofficeSecurityAccessor.BackofficeSecurity.CurrentUser;
            var result = _umbracoMapper.Map<UserDetail>(user);

            //set their remaining seconds
            result.SecondsUntilTimeout = HttpContext.User.GetRemainingAuthSeconds();

            return result;
        }

        /// <summary>
        /// When a user is invited they are not approved but we need to resolve the partially logged on (non approved)
        /// user.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// We cannot user GetCurrentUser since that requires they are approved, this is the same as GetCurrentUser but doesn't require them to be approved
        /// </remarks>
        [UmbracoBackOfficeAuthorize(redirectToUmbracoLogin: false, requireApproval: false)]
        [SetAngularAntiForgeryTokens]
        public ActionResult<UserDetail> GetCurrentInvitedUser()
        {
            var user = _backofficeSecurityAccessor.BackofficeSecurity.CurrentUser;

            if (user.IsApproved)
            {
                // if they are approved, than they are no longer invited and we can return an error
                return Forbid();
            }

            var result = _umbracoMapper.Map<UserDetail>(user);

            // set their remaining seconds
            result.SecondsUntilTimeout = HttpContext.User.GetRemainingAuthSeconds();

            return result;
        }

        /// <summary>
        /// Logs a user in
        /// </summary>
        /// <returns></returns>
        [SetAngularAntiForgeryTokens]
        public async Task<UserDetail> PostLogin(LoginModel loginModel)
        {
            // Sign the user in with username/password, this also gives a chance for developers to
            // custom verify the credentials and auto-link user accounts with a custom IBackOfficePasswordChecker
            var result = await _signInManager.PasswordSignInAsync(
                loginModel.Username, loginModel.Password, isPersistent: true, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                // return the user detail
                return GetUserDetail(_userService.GetByUsername(loginModel.Username));
            }

            if (result.RequiresTwoFactor)
            {
                throw new NotImplementedException("Implement MFA/2FA, we need to have some IOptions or similar to configure this");

                //var twofactorOptions = UserManager as IUmbracoBackOfficeTwoFactorOptions;
                //if (twofactorOptions == null)
                //{
                //    throw new HttpResponseException(
                //        Request.CreateErrorResponse(
                //            HttpStatusCode.BadRequest,
                //            "UserManager does not implement " + typeof(IUmbracoBackOfficeTwoFactorOptions)));
                //}

                //var twofactorView = twofactorOptions.GetTwoFactorView(
                //    owinContext,
                //    UmbracoContext,
                //    loginModel.Username);

                //if (twofactorView.IsNullOrWhiteSpace())
                //{
                //    throw new HttpResponseException(
                //        Request.CreateErrorResponse(
                //            HttpStatusCode.BadRequest,
                //            typeof(IUmbracoBackOfficeTwoFactorOptions) + ".GetTwoFactorView returned an empty string"));
                //}

                //var attemptedUser = Services.UserService.GetByUsername(loginModel.Username);

                //// create a with information to display a custom two factor send code view
                //var verifyResponse = Request.CreateResponse(HttpStatusCode.PaymentRequired, new
                //{
                //    twoFactorView = twofactorView,
                //    userId = attemptedUser.Id
                //});

                //_userManager.RaiseLoginRequiresVerificationEvent(User, attemptedUser.Id);

                //return verifyResponse;
            }

            // return BadRequest (400), we don't want to return a 401 because that get's intercepted
            // by our angular helper because it thinks that we need to re-perform the request once we are
            // authorized and we don't want to return a 403 because angular will show a warning message indicating
            // that the user doesn't have access to perform this function, we just want to return a normal invalid message.
            throw new HttpResponseException(HttpStatusCode.BadRequest);
        }

        /// <summary>
        /// Processes a password reset request.  Looks for a match on the provided email address
        /// and if found sends an email with a link to reset it
        /// </summary>
        /// <returns></returns>
        [SetAngularAntiForgeryTokens]
        public async Task<IActionResult> PostRequestPasswordReset(RequestPasswordResetModel model)
        {
            // If this feature is switched off in configuration the UI will be amended to not make the request to reset password available.
            // So this is just a server-side secondary check.
            if (_securitySettings.AllowPasswordReset == false)
                return BadRequest();

            var identityUser = await _userManager.FindByEmailAsync(model.Email);
            if (identityUser != null)
            {
                var user = _userService.GetByEmail(model.Email);
                if (user != null)
                {
                    var from = _globalSettings.Smtp.From;
                    var code = await _userManager.GeneratePasswordResetTokenAsync(identityUser);
                    var callbackUrl = ConstructCallbackUrl(identityUser.Id, code);

                    var message = _textService.Localize("resetPasswordEmailCopyFormat",
                        // Ensure the culture of the found user is used for the email!
                        UmbracoUserExtensions.GetUserCulture(identityUser.Culture, _textService, _globalSettings),
                        new[] { identityUser.UserName, callbackUrl });

                    var subject = _textService.Localize("login/resetPasswordEmailCopySubject",
                        // Ensure the culture of the found user is used for the email!
                        UmbracoUserExtensions.GetUserCulture(identityUser.Culture, _textService, _globalSettings));

                    var mailMessage = new EmailMessage(from, user.Email, subject, message, true);

                    await _emailSender.SendAsync(mailMessage);

                    _userManager.RaiseForgotPasswordRequestedEvent(User, user.Id);
                }
            }

            return Ok();
        }

        /// <summary>
        /// Processes a set password request.  Validates the request and sets a new password.
        /// </summary>
        /// <returns></returns>
        [SetAngularAntiForgeryTokens]
        public async Task<IActionResult> PostSetPassword(SetPasswordModel model)
        {
            var identityUser = await _userManager.FindByIdAsync(model.UserId.ToString());

            var result = await _userManager.ResetPasswordAsync(identityUser, model.ResetCode, model.Password);
            if (result.Succeeded)
            {
                var lockedOut = await _userManager.IsLockedOutAsync(identityUser);
                if (lockedOut)
                {
                    _logger.LogInformation("User {UserId} is currently locked out, unlocking and resetting AccessFailedCount", model.UserId);

                    //// var user = await UserManager.FindByIdAsync(model.UserId);
                    var unlockResult = await _userManager.SetLockoutEndDateAsync(identityUser, DateTimeOffset.Now);
                    if (unlockResult.Succeeded == false)
                    {
                        _logger.LogWarning("Could not unlock for user {UserId} - error {UnlockError}", model.UserId, unlockResult.Errors.First().Description);
                    }

                    var resetAccessFailedCountResult = await _userManager.ResetAccessFailedCountAsync(identityUser);
                    if (resetAccessFailedCountResult.Succeeded == false)
                    {
                        _logger.LogWarning("Could not reset access failed count {UserId} - error {UnlockError}", model.UserId, unlockResult.Errors.First().Description);
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
                    var user = _userService.GetByUsername(identityUser.UserName);
                    // also check InvitedDate and never logged in, otherwise this would allow a disabled user to reactivate their account with a forgot password
                    if (user.LastLoginDate == default && user.InvitedDate != null)
                    {
                        user.IsApproved = true;
                        user.InvitedDate = null;
                        _userService.Save(user);
                    }
                }

                _userManager.RaiseForgotPasswordChangedSuccessEvent(User, model.UserId);
                return Ok();
            }

            return new ValidationErrorResult(result.Errors.Any() ? result.Errors.First().Description : "Set password failed");
        }

        /// <summary>
        /// Logs the current user out
        /// </summary>
        /// <returns></returns>
        [ValidateAngularAntiForgeryToken]
        public IActionResult PostLogout()
        {
            HttpContext.SignOutAsync(Core.Constants.Security.BackOfficeAuthenticationType);

            _logger.LogInformation("User {UserName} from IP address {RemoteIpAddress} has logged out", User.Identity == null ? "UNKNOWN" : User.Identity.Name, HttpContext.Connection.RemoteIpAddress);

            _userManager.RaiseLogoutSuccessEvent(User, int.Parse(User.Identity.GetUserId()));

            return Ok();
        }



        /// <summary>
        /// Return the <see cref="UserDetail"/> for the given <see cref="IUser"/>
        /// </summary>
        /// <param name="user"></param>
        /// <param name="principal"></param>
        /// <returns></returns>
        private UserDetail GetUserDetail(IUser user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            var userDetail = _umbracoMapper.Map<UserDetail>(user);
            // update the userDetail and set their remaining seconds
            userDetail.SecondsUntilTimeout = TimeSpan.FromMinutes(_globalSettings.TimeOutInMinutes).TotalSeconds;

            return userDetail;
        }

        private string ConstructCallbackUrl(int userId, string code)
        {
            // Get an mvc helper to get the url
            var action = _linkGenerator.GetPathByAction(nameof(BackOfficeController.ValidatePasswordResetCode), ControllerExtensions.GetControllerName<BackOfficeController>(),
                new
                {
                    area = Constants.Web.Mvc.BackOfficeArea,
                    u = userId,
                    r = code
                });

            // Construct full URL using configured application URL (which will fall back to request)
            var applicationUri = _requestAccessor.GetApplicationUrl();
            var callbackUri = new Uri(applicationUri, action);
            return callbackUri.ToString();
        }
    }
}
