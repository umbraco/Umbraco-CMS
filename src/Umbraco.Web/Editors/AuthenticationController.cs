using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Helpers;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Security;
using AutoMapper;
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

        /// <summary>
        /// Checks if the current user's cookie is valid and if so returns OK or a 400 (BadRequest)
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage IsAuthenticated()
        {
            var attempt = UmbracoContext.Security.AuthorizeRequest();
            if (attempt == ValidateRequestAttempt.Success)
            {
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            //return BadRequest (400), we don't want to return a 401 because that get's intercepted 
            // by our angular helper because it thinks that we need to re-perform the request once we are
            // authorized and we don't want to return a 403 because angular will show a warning msg indicating 
            // that the user doesn't have access to perform this function, we just want to return a normal invalid msg.
            return Request.CreateResponse(HttpStatusCode.BadRequest);
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

        /// <summary>
        /// Logs a user in
        /// </summary>
        /// <returns></returns>
        [SetAngularAntiForgeryTokens]
        public UserDetail PostLogin(LoginModel loginModel)
        {
            if (UmbracoContext.Security.ValidateBackOfficeCredentials(loginModel.Username, loginModel.Password))
            {
                var user = Security.GetBackOfficeUser(loginModel.Username);

                //TODO: Clean up the int cast!
                var ticket = UmbracoContext.Security.PerformLogin(user);

                var http = this.TryGetHttpContext();
                if (http.Success == false)
                {
                    throw new InvalidOperationException("This method requires that an HttpContext be active");
                }
                http.Result.AuthenticateCurrentRequest(ticket, false);

                var result = Mapper.Map<UserDetail>(user);
                //set their remaining seconds
                result.SecondsUntilTimeout = ticket.GetRemainingAuthSeconds();
                return result;
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
    }
}