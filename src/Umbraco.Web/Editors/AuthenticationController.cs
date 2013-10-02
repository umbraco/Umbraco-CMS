using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using AutoMapper;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Models.Mapping;
using Umbraco.Web.Mvc;
using Umbraco.Web.Security;
using Umbraco.Web.WebApi;
using Umbraco.Web.WebApi.Filters;

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
        /// Checks if the current user's cookie is valid and if so returns the user object associated
        /// </summary>
        /// <returns></returns>
        public UserDetail GetCurrentUser()
        {
            var attempt = UmbracoContext.Security.AuthorizeRequest();
            if (attempt == ValidateRequestAttempt.Success)
            {
                var user = Services.UserService.GetUserById(UmbracoContext.Security.GetUserId());
                return Mapper.Map<UserDetail>(user);
            }

            //return Unauthorized (401) because the user is not authorized right now
            throw new HttpResponseException(HttpStatusCode.Unauthorized);
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
                var user = Services.UserService.GetUserByUserName(username);
                //TODO: Clean up the int cast!
                UmbracoContext.Security.PerformLogin((int)user.Id);
                return Mapper.Map<UserDetail>(user);
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