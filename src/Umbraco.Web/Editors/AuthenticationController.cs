using System.Collections.Generic;
using System.Net;
using System.Web;
using System.Web.Http;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Models.Mapping;
using Umbraco.Web.Mvc;
using Umbraco.Web.Security;
using Umbraco.Web.WebApi;

namespace Umbraco.Web.Editors
{

    /// <summary>
    /// The API controller used for editing content
    /// </summary>
    [PluginController("UmbracoApi")]
    [ValidationFilter]
    public class AuthenticationController : UmbracoApiController
    {
        private readonly UserModelMapper _userModelMapper;

        public AuthenticationController()
            : this(new UserModelMapper())
        {            
        }

        internal AuthenticationController(UserModelMapper userModelMapper)
        {
            _userModelMapper = userModelMapper;
        }

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
                var user =
                    Services.UserService.GetUserById(
                        UmbracoContext.Security.GetUserId(UmbracoContext.Security.UmbracoUserContextId));
                return _userModelMapper.ToUserDetail(user);
            }

            throw new HttpResponseException(HttpStatusCode.Unauthorized);
        }

        public UserDetail PostLogin(string username, string password)
        {
            if (UmbracoContext.Security.ValidateBackOfficeCredentials(username, password))
            {
                var user = Services.UserService.GetUserByUserName(username);
                //TODO: Clean up the int cast!
                UmbracoContext.Security.PerformLogin((int)user.Id);
                return _userModelMapper.ToUserDetail(user);
            }
            throw new HttpResponseException(HttpStatusCode.Unauthorized);
        }

        //public HttpResponseMessage PostLogout()
        //{

        //}
    }
}