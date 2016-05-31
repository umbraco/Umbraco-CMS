using System;
using System.Net;
using Umbraco.Core;
using Umbraco.Web.Mvc;

namespace Umbraco.Web.Editors
{
    /// <summary>
    /// API controller used for getting Gravatar urls
    /// </summary>
    [PluginController("UmbracoApi")]
    public class GravatarController : UmbracoAuthorizedJsonController
    {
        public string GetCurrentUserGravatarUrl()
        {
            var userService = Services.UserService;
            var user = userService.GetUserById(UmbracoContext.Security.CurrentUser.Id);
            var gravatarHash = user.Email.ToMd5();
            var gravatarUrl = "https://www.gravatar.com/avatar/" + gravatarHash;
           
            // Test if we can reach this URL, will fail when there's network or firewall errors
            var request = (HttpWebRequest)WebRequest.Create(gravatarUrl);
            // Require response within 10 seconds
            request.Timeout = 10000;
            try
            {
                using ((HttpWebResponse)request.GetResponse()) { }
            }
            catch (Exception)
            {
                // There was an HTTP or other error, return an null instead
                return null;
            }

            return gravatarUrl;
        }
    }
}
