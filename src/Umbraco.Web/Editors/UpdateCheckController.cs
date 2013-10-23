using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using Umbraco.Core.Configuration;
using Umbraco.Web.Models;

namespace Umbraco.Web.Editors
{
    public class UpdateCheckController : UmbracoAuthorizedJsonController
    {
        [UpdateCheckResponseFilter]
        public UpgradeCheckResponse GetCheck()
        {
            var updChkCookie = Request.Headers.GetCookies("UMB_UPDCHK").FirstOrDefault();
            var updateCheckCookie = updChkCookie != null ? updChkCookie["UMB_UPDCHK"].Value : "";            
            if (GlobalSettings.VersionCheckPeriod > 0 && string.IsNullOrEmpty(updateCheckCookie) && Security.CurrentUser.UserType.Alias == "admin")
            {
                var check = new global::umbraco.presentation.org.umbraco.update.CheckForUpgrade();
                var result = check.CheckUpgrade(UmbracoVersion.Current.Major,
                                                UmbracoVersion.Current.Minor,
                                                UmbracoVersion.Current.Build,
                                                UmbracoVersion.CurrentComment);     
         
                return new UpgradeCheckResponse(result.UpgradeType.ToString(), result.Comment, result.UpgradeUrl);
            }
            return null;
        }

        /// <summary>
        /// Adds the cookie response if it was successful
        /// </summary>
        /// <remarks>
        /// A filter is required because we are returning an object from the get method and not an HttpResponseMessage
        /// </remarks>
        internal class UpdateCheckResponseFilter : ActionFilterAttribute
        {
            public override void OnActionExecuted(HttpActionExecutedContext context)
            {
                var objectContent = context.Response.Content as ObjectContent;
                if (objectContent == null || objectContent.Value == null) return;

                //there is a result, set the outgoing cookie
                var cookie = new CookieHeaderValue("UMB_UPDCHK", "1")
                    {
                        Expires = DateTimeOffset.Now.AddDays(GlobalSettings.VersionCheckPeriod),
                        HttpOnly = true,
                        Secure = GlobalSettings.UseSSL
                    };
                context.Response.Headers.AddCookies(new[] { cookie });
            }
        }
    }
}
