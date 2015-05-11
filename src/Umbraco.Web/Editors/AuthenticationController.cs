using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Security;
using AutoMapper;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models.Membership;
using Umbraco.Web.Models;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Models.Mapping;
using Umbraco.Web.Mvc;
using Umbraco.Core.Security;
using Umbraco.Web.Security;
using Umbraco.Web.WebApi;
using Umbraco.Web.WebApi.Filters;
using umbraco.providers;
using Microsoft.AspNet.Identity.Owin;
using Umbraco.Core.Logging;
using Newtonsoft.Json.Linq;
using Umbraco.Core.Models.Identity;
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

        /// <summary>
        /// This is a special method that will return the current users' remaining session seconds, the reason
        /// it is special is because this route is ignored in the UmbracoModule so that the auth ticket doesn't get
        /// updated with a new timeout.
        /// </summary>
        /// <returns></returns>
        [WebApi.UmbracoAuthorize]
        [ValidateAngularAntiForgeryToken]
        public double GetRemainingTimeoutSeconds()
        {
            var httpContextAttempt = TryGetHttpContext();
            if (httpContextAttempt.Success)
            {
                return httpContextAttempt.Result.GetRemainingAuthSeconds();
            }

            //we need an http context
            throw new NotSupportedException("An HttpContext is required for this request");
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
                await SignInAsync(user, isPersistent: false);
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
        [HttpGet]
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
            var http = this.TryGetHttpContext();
            if (http.Success == false)
                throw new InvalidOperationException("This method requires that an HttpContext be active");

            if (UmbracoContext.Security.ValidateBackOfficeCredentials(loginModel.Username, loginModel.Password))
            {
                //get the user
                var user = Security.GetBackOfficeUser(loginModel.Username);
                var userDetail = Mapper.Map<UserDetail>(user);

                //create a response with the userDetail object
                var response = Request.CreateResponse(HttpStatusCode.OK, userDetail);

                //set the response cookies with the ticket (NOTE: This needs to be done with the custom webapi extension because
                // we cannot mix HttpContext.Response.Cookies and the way WebApi/Owin work)
                var ticket = response.UmbracoLoginWebApi(user);

                //Identity does some of it's own checks as well so we need to use it's sign in process too... this will essentially re-create the
                // ticket/cookie above but we need to create the ticket now so we can assign the Current Thread User/IPrinciple below                
                await SignInAsync(Mapper.Map<IUser, BackOfficeIdentityUser>(user), isPersistent: true);
                //This ensure the current principal is set, otherwise any logic executing after this wouldn't actually be authenticated
                http.Result.AuthenticateCurrentRequest(ticket, false);
                
                //update the userDetail and set their remaining seconds
                userDetail.SecondsUntilTimeout = ticket.GetRemainingAuthSeconds();

                return response;
            }

            //return BadRequest (400), we don't want to return a 401 because that get's intercepted 
            // by our angular helper because it thinks that we need to re-perform the request once we are
            // authorized and we don't want to return a 403 because angular will show a warning msg indicating 
            // that the user doesn't have access to perform this function, we just want to return a normal invalid msg.            
            throw new HttpResponseException(HttpStatusCode.BadRequest);
        }


        /// <summary>
        /// Logs the current user out
        /// </summary>
        /// <returns></returns>
        [UmbracoBackOfficeLogout]
        [ClearAngularAntiForgeryToken]
        [ValidateAngularAntiForgeryToken]
        public HttpResponseMessage PostLogout()
        {           
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        private void AddModelErrors(IdentityResult result, string prefix = "")
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(prefix, error);
            }
        }

        private async Task SignInAsync(BackOfficeIdentityUser user, bool isPersistent)
        {
            var owinContext = TryGetOwinContext().Result;

            owinContext.Authentication.SignOut(Core.Constants.Security.BackOfficeExternalAuthenticationType);

            owinContext.Authentication.SignIn(
                new AuthenticationProperties() { IsPersistent = isPersistent },
                await user.GenerateUserIdentityAsync(UserManager));
        }

    }
}