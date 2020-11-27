using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using Semver;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;

namespace Umbraco.Web.Editors
{
    [PluginController("UmbracoApi")]
    public class UpdateCheckController : UmbracoAuthorizedJsonController
    {
        private readonly IUpgradeService _upgradeService;

        public UpdateCheckController() { }

        public UpdateCheckController(IUpgradeService upgradeService)
        {
            _upgradeService = upgradeService;
        }

        [UpdateCheckResponseFilter]
        public async Task<UpgradeCheckResponse> GetCheck()
        {
            var updChkCookie = Request.Headers.GetCookies("UMB_UPDCHK").FirstOrDefault();
            var updateCheckCookie = updChkCookie != null ? updChkCookie["UMB_UPDCHK"].Value : "";
            if (GlobalSettings.VersionCheckPeriod > 0 && string.IsNullOrEmpty(updateCheckCookie) && Security.CurrentUser.IsAdmin())
            {
                try
                {
                    var version = new SemVersion(UmbracoVersion.Current.Major, UmbracoVersion.Current.Minor,
                        UmbracoVersion.Current.Build, UmbracoVersion.Comment);
                    var result = await _upgradeService.CheckUpgrade(version);

                    return new UpgradeCheckResponse(result.UpgradeType, result.Comment, result.UpgradeUrl);
                }
                catch (System.Net.WebException)
                {
                    //this occurs if the server is down or cannot be reached
                    return null;
                }
                catch (System.Web.Services.Protocols.SoapException)
                {
                    //this occurs if the server has a timeout
                    return null;
                }
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
                if (context.Response == null) return;

                var objectContent = context.Response.Content as ObjectContent;
                if (objectContent == null || objectContent.Value == null) return;

                //there is a result, set the outgoing cookie
                var cookie = new CookieHeaderValue("UMB_UPDCHK", "1")
                    {
                        Path = "/",
                        Expires = DateTimeOffset.Now.AddDays(Current.Configs.Global().VersionCheckPeriod),
                        HttpOnly = true,
                        Secure = Current.Configs.Global().UseHttps
                    };
                context.Response.Headers.AddCookies(new[] { cookie });
            }
        }
    }
}
