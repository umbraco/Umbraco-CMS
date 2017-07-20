using System.Net;
using System.Web.Http;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi.Filters;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.Editors
{
    [PluginController("UmbracoApi")]
    [UmbracoApplicationAuthorize(Constants.Applications.Users)]
    public class UserController : UmbracoAuthorizedJsonController
    {
        /// <summary>
        /// Disables the user with the given user id
        /// </summary>
        /// <param name="userId"></param>
        public bool PostDisableUser([FromUri]int userId)
        {
            var user = Services.UserService.GetUserById(userId);
            if (user == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            //without the permanent flag, this will just disable
            Services.UserService.Delete(user);
            return true;
        }
    }
}
