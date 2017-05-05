﻿using System;
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
using Umbraco.Core.Models.Identity;
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

        private BackOfficeUserManager<BackOfficeIdentityUser> _userManager;
        private BackOfficeSignInManager _signInManager;
        protected BackOfficeUserManager<BackOfficeIdentityUser> UserManager
        {
            get { return _userManager ?? (_userManager = TryGetOwinContext().Result.GetBackOfficeUserManager()); }
        }
        protected BackOfficeSignInManager SignInManager
        {
            get { return _signInManager ?? (_signInManager = TryGetOwinContext().Result.GetBackOfficeSignInManager()); }
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
                    return SetPrincipalAndReturnUserDetail(user);
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

                    var attemptedUser = Security.GetBackOfficeUser(loginModel.Username);
                    
                    //create a with information to display a custom two factor send code view
                    var verifyResponse = Request.CreateResponse(HttpStatusCode.PaymentRequired, new
                    {
                        twoFactorView = twofactorView,
                        userId = attemptedUser.Id
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
                    var callbackUrl = ConstructCallbackUrl(identityUser.Id, code);

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

        /// <summary>
        /// Used to retrived the 2FA providers for code submission
        /// </summary>
        /// <returns></returns>
        [SetAngularAntiForgeryTokens]
        public async Task<IEnumerable<string>> Get2FAProviders()
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId < 0)
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
            if (userId < 0)
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
            switch (result)
            {
                case SignInStatus.Success:
                    //get the user
                    var user = Security.GetBackOfficeUser(userName);
                    return SetPrincipalAndReturnUserDetail(user);
                case SignInStatus.LockedOut:
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
            Request.TryGetOwinContext().Result.Authentication.SignOut(
                Core.Constants.Security.BackOfficeAuthenticationType,
                Core.Constants.Security.BackOfficeExternalAuthenticationType);

            Logger.Info<AuthenticationController>("User {0} from IP address {1} has logged out",
                            () => User.Identity == null ? "UNKNOWN" : User.Identity.Name,
                            () => TryGetOwinContext().Result.Request.RemoteIpAddress);

            return Request.CreateResponse(HttpStatusCode.OK);
        }


        /// <summary>
        /// This is used when the user is auth'd successfully and we need to return an OK with user details along with setting the current Principal in the request
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private HttpResponseMessage SetPrincipalAndReturnUserDetail(IUser user)
        {
            if (user == null) throw new ArgumentNullException("user");

            var userDetail = Mapper.Map<UserDetail>(user);
            //update the userDetail and set their remaining seconds
            userDetail.SecondsUntilTimeout = TimeSpan.FromMinutes(GlobalSettings.TimeOutInMinutes).TotalSeconds;

            //create a response with the userDetail object
            var response = Request.CreateResponse(HttpStatusCode.OK, userDetail);

            //ensure the user is set for the current request
            Request.SetPrincipalForRequest(user);

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
                    area = GlobalSettings.UmbracoMvcArea,
                    u = userId,
                    r = code
                });

            // Construct full URL using configured application URL (which will fall back to request)
            var applicationUri = new Uri(ApplicationContext.UmbracoApplicationUrl);
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