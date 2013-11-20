using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Security;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models.Membership;
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
    public class AuthenticationController : UmbracoApiController
    {

        /// <summary>
        /// Remove the xml formatter... only support JSON!
        /// </summary>
        /// <param name="controllerContext"></param>
        protected override void Initialize(global::System.Web.Http.Controllers.HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            controllerContext.Configuration.Formatters.Remove(controllerContext.Configuration.Formatters.XmlFormatter);
        }
        
        /// <summary>
        /// This is a special method that will return the current users' remaining session seconds, the reason
        /// it is special is because this route is ignored in the UmbracoModule so that the auth ticket doesn't get
        /// updated with a new timeout.
        /// </summary>
        /// <returns></returns>
        [WebApi.UmbracoAuthorize]
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
        [WebApi.UmbracoAuthorize]
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
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public UserDetail PostLogin(string username, string password)
        {
            if (UmbracoContext.Security.ValidateBackOfficeCredentials(username, password))
            {
                var user = Security.GetBackOfficeUser(username);

                //TODO: Clean up the int cast!
                var timeoutSeconds = UmbracoContext.Security.PerformLogin((int)user.Id);
                var result = Mapper.Map<UserDetail>(user);
                //set their remaining seconds
                result.SecondsUntilTimeout = timeoutSeconds;
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
        public HttpResponseMessage PostLogout()
        {
            UmbracoContext.Security.ClearCurrentLogin();
            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}