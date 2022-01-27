using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Collections.Generic;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Identity;
using Umbraco.Core.Services;
using Umbraco.Web.Models;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Security;
using Umbraco.Web.WebApi;
using Umbraco.Web.WebApi.Filters;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Web.Composing;
using IUser = Umbraco.Core.Models.Membership.IUser;
using Umbraco.Web.Editors.Filters;
using Microsoft.Owin.Security;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Sync;

namespace Umbraco.Web.Editors
{
    /// <summary>
    /// The API controller used for editing content
    /// </summary>
    [Mvc.PluginController("UmbracoApi")]
    [ValidationFilter]
    [AngularJsonOnlyConfiguration]
    [IsBackOffice]
    [DisableBrowserCache]
    public class AuthenticationController : UmbracoApiController
    {
        private readonly IUmbracoSettingsSection _umbracoSettingsSection;
        private BackOfficeUserManager<BackOfficeIdentityUser> _userManager;
        private BackOfficeSignInManager _signInManager;

        public AuthenticationController(
            IGlobalSettings globalSettings,
            IUmbracoContextAccessor umbracoContextAccessor,
            ISqlContext sqlContext,
            ServiceContext services,
            AppCaches appCaches,
            IProfilingLogger logger,
            IRuntimeState runtimeState,
            UmbracoHelper umbracoHelper,
            IUmbracoSettingsSection umbracoSettingsSection)
            : base(globalSettings, umbracoContextAccessor, sqlContext, services, appCaches, logger, runtimeState, umbracoHelper, Current.Mapper)
        {
            _umbracoSettingsSection = umbracoSettingsSection;
        }

        protected BackOfficeUserManager<BackOfficeIdentityUser> UserManager => _userManager
            ?? (_userManager = TryGetOwinContext().Result.GetBackOfficeUserManager());

        protected BackOfficeSignInManager SignInManager => _signInManager
            ?? (_signInManager = TryGetOwinContext().Result.GetBackOfficeSignInManager());

        /// <summary>
        /// Returns the configuration for the backoffice user membership provider - used to configure the change password dialog
        /// </summary>
        /// <returns></returns>
        [WebApi.UmbracoAuthorize(requireApproval: false)]
        public IDictionary<string, object> GetMembershipProviderConfig()
        {
            // TODO: Check if the current PasswordValidator is an IMembershipProviderPasswordValidator, if
            // it's not than we should return some generic defaults
            var provider = Core.Security.MembershipProviderExtensions.GetUsersMembershipProvider();
            return provider.GetConfiguration(Services.UserService);
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
        [DenyLocalLoginAuthorization]
        public async Task<UserDisplay> PostVerifyInvite([FromUri]int id, [FromUri]string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));

            var decoded = token.FromUrlBase64();
            if (decoded.IsNullOrWhiteSpace())
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));

            var identityUser = await UserManager.FindByIdAsync(id);
            if (identityUser == null)
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));

            var result = await UserManager.ConfirmEmailAsync(id, decoded);

            if (result.Succeeded == false)
            {
                throw new HttpResponseException(Request.CreateNotificationValidationErrorResponse(string.Join(", ", result.Errors)));
            }

            Request.TryGetOwinContext().Result.Authentication.SignOut(
                Core.Constants.Security.BackOfficeAuthenticationType,
                Core.Constants.Security.BackOfficeExternalAuthenticationType);

            await SignInManager.SignInAsync(identityUser, false, false);

            var user = Services.UserService.GetUserById(id);

            return Mapper.Map<UserDisplay>(user);
        }

        [WebApi.UmbracoAuthorize]
        [ValidateAngularAntiForgeryToken]
        public async Task<HttpResponseMessage> PostUnLinkLogin(UnLinkLoginModel unlinkLoginModel)
        {
            var owinContext = TryGetOwinContext().Result;
            ExternalSignInAutoLinkOptions autoLinkOptions = null;
            var authType = owinContext.Authentication.GetExternalAuthenticationTypes().FirstOrDefault(x => x.AuthenticationType == unlinkLoginModel.LoginProvider);
            if (authType == null)
            {
                Logger.Warn<BackOfficeController>("Could not find external authentication provider registered: {LoginProvider}", unlinkLoginModel.LoginProvider);
            }
            else
            {
                autoLinkOptions = authType.GetExternalSignInAutoLinkOptions();
                if (!autoLinkOptions.AllowManualLinking)
                {
                    // If AllowManualLinking is disabled for this provider we cannot unlink
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
                }
            }

            var result = await UserManager.RemoveLoginAsync(
                User.Identity.GetUserId<int>(),
                new UserLoginInfo(unlinkLoginModel.LoginProvider, unlinkLoginModel.ProviderKey));

            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId<int>());
                await SignInManager.SignInAsync(user, isPersistent: true, rememberBrowser: false);
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            else
            {
                AddModelErrors(result);
                return Request.CreateValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Checks if the current user's cookie is valid and if so returns OK or a 400 (BadRequest)
        /// </summary>
        /// <returns></returns>
        [System.Web.Http.HttpGet]
        public bool IsAuthenticated()
        {
            var attempt = UmbracoContext.Security.AuthorizeRequest();
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
        [WebApi.UmbracoAuthorize]
        [SetAngularAntiForgeryTokens]
        [CheckIfUserTicketDataIsStale]
        public UserDetail GetCurrentUser()
        {
            var user = UmbracoContext.Security.CurrentUser;
            var result = Mapper.Map<UserDetail>(user);
            var httpContextAttempt = TryGetHttpContext();
            if (httpContextAttempt.Success)
            {
                //set their remaining seconds
                result.SecondsUntilTimeout = httpContextAttempt.Result.GetRemainingAuthSeconds();
            }

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
        [WebApi.UmbracoAuthorize(requireApproval: false)]
        [SetAngularAntiForgeryTokens]
        public UserDetail GetCurrentInvitedUser()
        {
            var user = UmbracoContext.Security.CurrentUser;

            if (user.IsApproved)
            {
                // if they are approved, than they are no longer invited and we can return an error
                throw new HttpResponseException(Request.CreateUserNoAccessResponse());
            }

            var result = Mapper.Map<UserDetail>(user);
            var httpContextAttempt = TryGetHttpContext();
            if (httpContextAttempt.Success)
            {
                // set their remaining seconds
                result.SecondsUntilTimeout = httpContextAttempt.Result.GetRemainingAuthSeconds();
            }

            return result;
        }

        // TODO: This should be on the CurrentUserController?
        [WebApi.UmbracoAuthorize]
        [ValidateAngularAntiForgeryToken]
        public async Task<Dictionary<string, string>> GetCurrentUserLinkedLogins()
        {
            var identityUser = await UserManager.FindByIdAsync(UmbracoContext.Security.GetUserId().ResultOr(0));
            var result = new Dictionary<string, string>();
            foreach (var l in identityUser.Logins)
            {
                result[l.LoginProvider] = l.ProviderKey;
            }
            return result;
        }

        /// <summary>
        /// Logs a user in
        /// </summary>
        /// <returns></returns>
        [SetAngularAntiForgeryTokens]
        [DenyLocalLoginAuthorization]
        public async Task<HttpResponseMessage> PostLogin(LoginModel loginModel)
        {
            var http = EnsureHttpContext();
            var owinContext = TryGetOwinContext().Result;

            // Sign the user in with username/password, this also gives a chance for developers to
            // custom verify the credentials and auto-link user accounts with a custom IBackOfficePasswordChecker
            var result = await SignInManager.PasswordSignInAsync(
                loginModel.Username, loginModel.Password, isPersistent: true, shouldLockout: true);

            switch (result)
            {
                case SignInStatus.Success:

                    // get the user
                    var user = Services.UserService.GetByUsername(loginModel.Username);
                    UserManager.RaiseLoginSuccessEvent(user.Id);

                    return SetPrincipalAndReturnUserDetail(user, owinContext.Request.User);
                case SignInStatus.RequiresVerification:

                    var twofactorOptions = UserManager as IUmbracoBackOfficeTwoFactorOptions;
                    if (twofactorOptions == null)
                    {
                        throw new HttpResponseException(
                            Request.CreateErrorResponse(
                                HttpStatusCode.BadRequest,
                                "UserManager does not implement " + typeof(IUmbracoBackOfficeTwoFactorOptions)));
                    }

                    var twofactorView = twofactorOptions.GetTwoFactorView(
                        owinContext,
                        UmbracoContext,
                        loginModel.Username);

                    if (twofactorView.IsNullOrWhiteSpace())
                    {
                        throw new HttpResponseException(
                            Request.CreateErrorResponse(
                                HttpStatusCode.BadRequest,
                                typeof(IUmbracoBackOfficeTwoFactorOptions) + ".GetTwoFactorView returned an empty string"));
                    }

                    var attemptedUser = Services.UserService.GetByUsername(loginModel.Username);

                    // create a with information to display a custom two factor send code view
                    var verifyResponse = Request.CreateResponse(HttpStatusCode.PaymentRequired, new
                    {
                        twoFactorView = twofactorView,
                        userId = attemptedUser.Id
                    });

                    UserManager.RaiseLoginRequiresVerificationEvent(attemptedUser.Id);

                    return verifyResponse;

                case SignInStatus.LockedOut:
                case SignInStatus.Failure:
                default:
                    // return BadRequest (400), we don't want to return a 401 because that get's intercepted
                    // by our angular helper because it thinks that we need to re-perform the request once we are
                    // authorized and we don't want to return a 403 because angular will show a warning message indicating
                    // that the user doesn't have access to perform this function, we just want to return a normal invalid message.
                    throw new HttpResponseException(HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Processes a password reset request.  Looks for a match on the provided email address
        /// and if found sends an email with a link to reset it
        /// </summary>
        /// <returns></returns>
        [SetAngularAntiForgeryTokens]
        [DenyLocalLoginAuthorization]
        public async Task<HttpResponseMessage> PostRequestPasswordReset(RequestPasswordResetModel model)
        {
            // If this feature is switched off in configuration the UI will be amended to not make the request to reset password available.
            // So this is just a server-side secondary check.
            if (Current.Configs.Settings().Security.AllowPasswordReset == false)
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }
            var identityUser = await SignInManager.UserManager.FindByEmailAsync(model.Email);
            if (identityUser != null)
            {
                var user = Services.UserService.GetByEmail(model.Email);
                if (user != null)
                {
                    var code = await UserManager.GeneratePasswordResetTokenAsync(identityUser.Id);
                    var callbackUrl = ConstructCallbackUrl(identityUser.Id, code);

                    var message = Services.TextService.Localize("resetPasswordEmailCopyFormat",
                        // Ensure the culture of the found user is used for the email!
                        UserExtensions.GetUserCulture(identityUser.Culture, Services.TextService, GlobalSettings),
                        new[] { identityUser.UserName, callbackUrl });

                    await UserManager.SendEmailAsync(identityUser.Id,
                        Services.TextService.Localize("login", "resetPasswordEmailCopySubject",
                            // Ensure the culture of the found user is used for the email!
                            UserExtensions.GetUserCulture(identityUser.Culture, Services.TextService, GlobalSettings)),
                        message);

                    UserManager.RaiseForgotPasswordRequestedEvent(user.Id);
                }
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        /// <summary>
        /// Used to retrieve the 2FA providers for code submission
        /// </summary>
        /// <returns></returns>
        [SetAngularAntiForgeryTokens]
        public async Task<IEnumerable<string>> Get2FAProviders()
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == int.MinValue)
            {
                Logger.Warn<AuthenticationController>("Get2FAProviders :: No verified user found, returning 404");
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            return userFactors;
        }

        [SetAngularAntiForgeryTokens]
        public async Task<IHttpActionResult> PostSend2FACode([FromBody]string provider)
        {
            if (provider.IsNullOrWhiteSpace())
                throw new HttpResponseException(HttpStatusCode.NotFound);

            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == int.MinValue)
            {
                Logger.Warn<AuthenticationController>("Get2FAProviders :: No verified user found, returning 404");
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            // Generate the token and send it
            if (await SignInManager.SendTwoFactorCodeAsync(provider) == false)
            {
                return BadRequest("Invalid code");
            }
            return Ok();
        }

        [SetAngularAntiForgeryTokens]
        public async Task<HttpResponseMessage> PostVerify2FACode(Verify2FACodeModel model)
        {
            if (ModelState.IsValid == false)
            {
                return Request.CreateValidationErrorResponse(ModelState);
            }

            var userName = await SignInManager.GetVerifiedUserNameAsync();
            if (userName == null)
            {
                Logger.Warn<AuthenticationController>("Get2FAProviders :: No verified user found, returning 404");
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: true, rememberBrowser: false);
            var owinContext = TryGetOwinContext().Result;

            var user = Services.UserService.GetByUsername(userName);
            switch (result)
            {
                case SignInStatus.Success:
                    UserManager.RaiseLoginSuccessEvent(user.Id);
                    return SetPrincipalAndReturnUserDetail(user, owinContext.Request.User);
                case SignInStatus.LockedOut:
                    UserManager.RaiseAccountLockedEvent(user.Id);
                    return Request.CreateValidationErrorResponse("User is locked out");
                case SignInStatus.Failure:
                default:
                    return Request.CreateValidationErrorResponse("Invalid code");
            }
        }

        /// <summary>
        /// Processes a set password request.  Validates the request and sets a new password.
        /// </summary>
        /// <returns></returns>
        [SetAngularAntiForgeryTokens]
        public async Task<HttpResponseMessage> PostSetPassword(SetPasswordModel model)
        {
            var result = await UserManager.ResetPasswordAsync(model.UserId, model.ResetCode, model.Password);
            if (result.Succeeded)
            {
                var lockedOut = await UserManager.IsLockedOutAsync(model.UserId);
                if (lockedOut)
                {
                    Logger.Info<AuthenticationController,int>("User {UserId} is currently locked out, unlocking and resetting AccessFailedCount", model.UserId);

                    //// var user = await UserManager.FindByIdAsync(model.UserId);
                    var unlockResult = await UserManager.SetLockoutEndDateAsync(model.UserId, DateTimeOffset.Now);
                    if (unlockResult.Succeeded == false)
                    {
                        Logger.Warn<AuthenticationController, int, string>("Could not unlock for user {UserId} - error {UnlockError}", model.UserId, unlockResult.Errors.First());
                    }

                    var resetAccessFailedCountResult = await UserManager.ResetAccessFailedCountAsync(model.UserId);
                    if (resetAccessFailedCountResult.Succeeded == false)
                    {
                        Logger.Warn<AuthenticationController,int, string>("Could not reset access failed count {UserId} - error {UnlockError}", model.UserId, unlockResult.Errors.First());
                    }
                }

                // They've successfully set their password, we can now update their user account to be confirmed
                // if user was only invited, then they have not been approved
                // but a successful forgot password flow (e.g. if their token had expired and they did a forgot password instead of request new invite)
                // means we have verified their email
                if (!UserManager.IsEmailConfirmed(model.UserId))
                {
                    await UserManager.ConfirmEmailAsync(model.UserId, model.ResetCode);
                }

                // if the user is invited, enable their account on forgot password
                var identityUser = await UserManager.FindByIdAsync(model.UserId);
                // invited is not approved, never logged in, invited date present
                /*
                if (LastLoginDate == default && IsApproved == false && InvitedDate != null)
                    return UserState.Invited;
                */
                if (identityUser != null && !identityUser.IsApproved)
                {
                    var user = Services.UserService.GetByUsername(identityUser.UserName);
                    // also check InvitedDate and never logged in, otherwise this would allow a disabled user to reactivate their account with a forgot password
                    if (user.LastLoginDate == default && user.InvitedDate != null)
                    {
                        user.IsApproved = true;
                        user.InvitedDate = null;
                        Services.UserService.Save(user);
                    }
                }

                UserManager.RaiseForgotPasswordChangedSuccessEvent(model.UserId);
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            return Request.CreateValidationErrorResponse(
                result.Errors.Any() ? result.Errors.First() : "Set password failed");
        }


        /// <summary>
        /// Logs the current user out
        /// </summary>
        /// <returns></returns>
        [ClearAngularAntiForgeryToken]
        [ValidateAngularAntiForgeryToken]
        public HttpResponseMessage PostLogout()
        {
            var owinContext = Request.TryGetOwinContext().Result;

            owinContext.Authentication.SignOut(
                Core.Constants.Security.BackOfficeAuthenticationType,
                Core.Constants.Security.BackOfficeExternalAuthenticationType);

            Logger.Info<AuthenticationController, string, string>("User {UserName} from IP address {RemoteIpAddress} has logged out", User.Identity == null ? "UNKNOWN" : User.Identity.Name, owinContext.Request.RemoteIpAddress);

            if (UserManager != null)
            {
                int.TryParse(User.Identity.GetUserId(), out var userId);
                var args = UserManager.RaiseLogoutSuccessEvent(userId);
                if (!args.SignOutRedirectUrl.IsNullOrWhiteSpace())
                    return Request.CreateResponse(new
                    {
                        signOutRedirectUrl = args.SignOutRedirectUrl
                    });
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        /// <summary>
        /// This is used when the user is auth'd successfully and we need to return an OK with user details along with setting the current Principal in the request
        /// </summary>
        /// <param name="user"></param>
        /// <param name="principal"></param>
        /// <returns></returns>
        private HttpResponseMessage SetPrincipalAndReturnUserDetail(IUser user, IPrincipal principal)
        {
            if (user == null) throw new ArgumentNullException("user");
            if (principal == null) throw new ArgumentNullException(nameof(principal));

            var userDetail = Mapper.Map<UserDetail>(user);
            // update the userDetail and set their remaining seconds
            userDetail.SecondsUntilTimeout = TimeSpan.FromMinutes(GlobalSettings.TimeOutInMinutes).TotalSeconds;

            // create a response with the userDetail object
            var response = Request.CreateResponse(HttpStatusCode.OK, userDetail);

            // ensure the user is set for the current request
            Request.SetPrincipalForRequest(principal);

            return response;
        }

        private string ConstructCallbackUrl(int userId, string code)
        {
            // Get an mvc helper to get the url
            var http = EnsureHttpContext();
            var urlHelper = new UrlHelper(http.Request.RequestContext);
            var action = urlHelper.Action("ValidatePasswordResetCode", "BackOffice",
                new
                {
                    area = GlobalSettings.GetUmbracoMvcArea(),
                    u = userId,
                    r = code
                });

            // Construct full URL using configured application URL (which will fall back to current request)
            var applicationUri = ApplicationUrlHelper.GetApplicationUriUncached(http.Request, _umbracoSettingsSection, GlobalSettings);
            var callbackUri = new Uri(applicationUri, action);
            return callbackUri.ToString();
        }


        private HttpContextBase EnsureHttpContext()
        {
            var attempt = this.TryGetHttpContext();
            if (attempt.Success == false)
                throw new InvalidOperationException("This method requires that an HttpContext be active");
            return attempt.Result;
        }



        private void AddModelErrors(IdentityResult result, string prefix = "")
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(prefix, error);
            }
        }

    }
}
