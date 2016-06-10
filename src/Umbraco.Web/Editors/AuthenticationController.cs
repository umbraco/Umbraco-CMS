using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using AutoMapper;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Web.Models;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using Umbraco.Web.Security;
using Umbraco.Web.Security.Identity;
using Umbraco.Web.WebApi;
using Umbraco.Web.WebApi.Filters;
using IUser = Umbraco.Core.Models.Membership.IUser;

namespace Umbraco.Web.Editors
{
    /// <summary>
    /// The API controller used for editing content
    /// </summary>
    [PluginController("UmbracoApi")]
    [ValidationFilter]
    [AngularJsonOnlyConfiguration]
    [IsBackOffice]
    public class AuthenticationController : UmbracoApiController
    {

        private BackOfficeUserManager _userManager;
        private BackOfficeSignInManager _signInManager;

        protected BackOfficeUserManager UserManager
        {
            get
            {
                if (_userManager == null)
                {
                    var mgr = TryGetOwinContext().Result.GetUserManager<BackOfficeUserManager>();
                    if (mgr == null)
                    {
                        throw new NullReferenceException("Could not resolve an instance of " + typeof(BackOfficeUserManager) + " from the " + typeof(IOwinContext) + " GetUserManager method");
                    }
                    _userManager = mgr;
                }
                return _userManager;
            }
        }

        protected BackOfficeSignInManager SignInManager
        {
            get
            {
                if (_signInManager == null)
                {
                    var mgr = TryGetOwinContext().Result.Get<BackOfficeSignInManager>();
                    if (mgr == null)
                    {
                        throw new NullReferenceException("Could not resolve an instance of " + typeof(BackOfficeSignInManager) + " from the " + typeof(IOwinContext));
                    }
                    _signInManager = mgr;
                }
                return _signInManager;
            }
        }

        
        [WebApi.UmbracoAuthorize]
        [ValidateAngularAntiForgeryToken]
        public async Task<HttpResponseMessage> PostUnLinkLogin(UnLinkLoginModel unlinkLoginModel)
        {
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
        public UserDetail GetCurrentUser()
        {
            var user = Services.UserService.GetUserById(UmbracoContext.Security.GetUserId());
            var result = Mapper.Map<UserDetail>(user);
            var httpContextAttempt = TryGetHttpContext();
            if (httpContextAttempt.Success)
            {
                //set their remaining seconds
                result.SecondsUntilTimeout = httpContextAttempt.Result.GetRemainingAuthSeconds();
            }

            return result;
        }

        [WebApi.UmbracoAuthorize]
        [ValidateAngularAntiForgeryToken]
        public async Task<Dictionary<string, string>>  GetCurrentUserLinkedLogins()
        {
            var identityUser = await UserManager.FindByIdAsync(UmbracoContext.Security.GetUserId());
            return identityUser.Logins.ToDictionary(x => x.LoginProvider, x => x.ProviderKey);
        }

        /// <summary>
        /// Logs a user in
        /// </summary>
        /// <returns></returns>
        [SetAngularAntiForgeryTokens]
        public async Task<HttpResponseMessage> PostLogin(LoginModel loginModel)
        {
            var http = EnsureHttpContext();

            var result = await SignInManager.PasswordSignInAsync(
                loginModel.Username, loginModel.Password, isPersistent: true, shouldLockout: true);

            switch (result)
            {
                case SignInStatus.Success:

                    //get the user
                    var user = Security.GetBackOfficeUser(loginModel.Username);
                    var userDetail = Mapper.Map<UserDetail>(user);
                    //update the userDetail and set their remaining seconds
                    userDetail.SecondsUntilTimeout = TimeSpan.FromMinutes(GlobalSettings.TimeOutInMinutes).TotalSeconds;
                    
                    //create a response with the userDetail object
                    var response = Request.CreateResponse(HttpStatusCode.OK, userDetail);

                    //ensure the user is set for the current request
                    Request.SetPrincipalForRequest(user);

                    return response;

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
                        TryGetOwinContext().Result,
                        UmbracoContext,
                        loginModel.Username);

                    if (twofactorView.IsNullOrWhiteSpace())
                    {
                        throw new HttpResponseException(
                            Request.CreateErrorResponse(
                                HttpStatusCode.BadRequest,
                                typeof(IUmbracoBackOfficeTwoFactorOptions) + ".GetTwoFactorView returned an empty string"));
                    }

                    //create a with information to display a custom two factor send code view
                    var verifyResponse = Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        twoFactorView = twofactorView
                    });

                    return verifyResponse;

                case SignInStatus.LockedOut:
                case SignInStatus.Failure:
                default:
                    //return BadRequest (400), we don't want to return a 401 because that get's intercepted 
                    // by our angular helper because it thinks that we need to re-perform the request once we are
                    // authorized and we don't want to return a 403 because angular will show a warning msg indicating 
                    // that the user doesn't have access to perform this function, we just want to return a normal invalid msg.            
                    throw new HttpResponseException(HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Processes a password reset request.  Looks for a match on the provided email address
        /// and if found sends an email with a link to reset it
        /// </summary>
        /// <returns></returns>
        [SetAngularAntiForgeryTokens]
        public async Task<HttpResponseMessage> PostRequestPasswordReset(RequestPasswordResetModel model)
        {
            // If this feature is switched off in configuration the UI will be amended to not make the request to reset password available.
            // So this is just a server-side secondary check.
            if (UmbracoConfig.For.UmbracoSettings().Security.AllowPasswordReset == false)
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }
            var identityUser = await SignInManager.UserManager.FindByEmailAsync(model.Email);
            if (identityUser != null)
            {
                var user = Services.UserService.GetByEmail(model.Email);
                if (user != null && user.IsLockedOut == false)
                {
                    var code = await UserManager.GeneratePasswordResetTokenAsync(identityUser.Id);
                    var callbackUrl = ConstuctCallbackUrl(identityUser.Id, code);

                    var message = Services.TextService.Localize("resetPasswordEmailCopyFormat",
                        //Ensure the culture of the found user is used for the email!
                        UserExtensions.GetUserCulture(identityUser.Culture, Services.TextService),
                        new[] {identityUser.UserName, callbackUrl});

                    await UserManager.SendEmailAsync(identityUser.Id,
                        Services.TextService.Localize("login/resetPasswordEmailCopySubject",
                            //Ensure the culture of the found user is used for the email!
                            UserExtensions.GetUserCulture(identityUser.Culture, Services.TextService)),
                        message);
                }
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        private string ConstuctCallbackUrl(int userId, string code)
        {
            //get an mvc helper to get the url
            var http = EnsureHttpContext();
            var urlHelper = new UrlHelper(http.Request.RequestContext);

            var action = urlHelper.Action("ValidatePasswordResetCode", "BackOffice", 
                new
                {
                    area = GlobalSettings.UmbracoMvcArea,
                    u = userId,
                    r = code
                });

            //TODO: Virtual path?

            return string.Format("{0}://{1}{2}",
                http.Request.Url.Scheme,
                http.Request.Url.Host + (http.Request.Url.Port == 80 ? string.Empty : ":" + http.Request.Url.Port),
                action);
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
                return Request.CreateResponse(HttpStatusCode.OK);
            }

            return Request.CreateValidationErrorResponse(
                result.Errors.Any() ? result.Errors.First() : "Set password failed");
        }

        private HttpContextBase EnsureHttpContext()
        {
            var attempt = this.TryGetHttpContext();
            if (attempt.Success == false)
                throw new InvalidOperationException("This method requires that an HttpContext be active");
            return attempt.Result;
        }

        /// <summary>
        /// Logs the current user out
        /// </summary>
        /// <returns></returns>
        [ClearAngularAntiForgeryToken]
        [ValidateAngularAntiForgeryToken]
        public HttpResponseMessage PostLogout()
        {
            Request.TryGetOwinContext().Result.Authentication.SignOut(
                Core.Constants.Security.BackOfficeAuthenticationType,
                Core.Constants.Security.BackOfficeExternalAuthenticationType);

            Logger.Info<AuthenticationController>("User {0} from IP address {1} has logged out",
                            () => User.Identity == null ? "UNKNOWN" : User.Identity.Name,
                            () => TryGetOwinContext().Result.Request.RemoteIpAddress);

            return Request.CreateResponse(HttpStatusCode.OK);
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