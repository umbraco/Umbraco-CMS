using System.Net;
using System.Web.Http;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Models.Mapping;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;

namespace Umbraco.Web.Editors
{
    /// <summary>
    /// The API controller used for editing content
    /// </summary>
    [PluginController("UmbracoApi")]
    [ValidationFilter]
    public class AuthenticationController : UmbracoAuthorizedApiController
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