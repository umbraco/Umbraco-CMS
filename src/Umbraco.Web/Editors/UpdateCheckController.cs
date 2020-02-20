using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using Semver;
using Umbraco.Core;
using Umbraco.Web.Composing;
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
        private readonly IUmbracoVersion _umbracoVersion;

        public UpdateCheckController() { }

        public UpdateCheckController(IUpgradeService upgradeService, IUmbracoVersion umbracoVersion)
        {
            _upgradeService = upgradeService;
            _umbracoVersion = umbracoVersion;
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
                    var version = new SemVersion(_umbracoVersion.Current.Major, _umbracoVersion.Current.Minor,
                        _umbracoVersion.Current.Build, _umbracoVersion.Comment);
                    var result = await _upgradeService.CheckUpgrade(version);

                    return new UpgradeCheckResponse(result.UpgradeType, result.Comment, result.UpgradeUrl, _umbracoVersion);
                }
                catch
                {
                    //We don't want to crash due to this
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
