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
        /// Simply checks if the current user's cookie is valid and if so returns the user object associated
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

            //return Forbidden (403), we don't want to return a 401 because that get's intercepted 
            // by our angular helper because it thinks that we need to re-perform the request once we are
            // authorized. A login form should not return a 401 because its the authorization process.
            throw new HttpResponseException(HttpStatusCode.Forbidden);
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