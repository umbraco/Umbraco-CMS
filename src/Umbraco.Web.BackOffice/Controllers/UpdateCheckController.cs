using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Semver;
using Umbraco.Composing;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Models;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Web.Common.Attributes;
using Umbraco.Web.Models;
using Umbraco.Web.Security;

namespace Umbraco.Web.BackOffice.Controllers
{
    [PluginController(Constants.Web.Mvc.BackOfficeApiArea)]
    public class UpdateCheckController : UmbracoAuthorizedJsonController
    {
        private readonly IUpgradeService _upgradeService;
        private readonly IUmbracoVersion _umbracoVersion;
        private readonly ICookieManager _cookieManager;
        private readonly IBackOfficeSecurityAccessor _backofficeSecurityAccessor;
        private readonly GlobalSettings _globalSettings;

        public UpdateCheckController(
            IUpgradeService upgradeService,
            IUmbracoVersion umbracoVersion,
            ICookieManager cookieManager,
            IBackOfficeSecurityAccessor backofficeSecurityAccessor,
            IOptions<GlobalSettings> globalSettings)
        {
            _upgradeService = upgradeService ?? throw new ArgumentNullException(nameof(upgradeService));
            _umbracoVersion = umbracoVersion ?? throw new ArgumentNullException(nameof(umbracoVersion));
            _cookieManager = cookieManager ?? throw new ArgumentNullException(nameof(cookieManager));
            _backofficeSecurityAccessor = backofficeSecurityAccessor ?? throw new ArgumentNullException(nameof(backofficeSecurityAccessor));
            _globalSettings = globalSettings.Value ?? throw new ArgumentNullException(nameof(globalSettings));
        }

        [UpdateCheckResponseFilter]
        public async Task<UpgradeCheckResponse> GetCheck()
        {
            var updChkCookie = _cookieManager.GetCookieValue("UMB_UPDCHK");
            var updateCheckCookie = updChkCookie ?? string.Empty;
            if (_globalSettings.VersionCheckPeriod > 0 && string.IsNullOrEmpty(updateCheckCookie) && _backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser.IsAdmin())
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
        ///
        internal class UpdateCheckResponseFilterAttribute : TypeFilterAttribute
        {
            public UpdateCheckResponseFilterAttribute() : base(typeof(UpdateCheckResponseFilter))
            {
            }

            private class UpdateCheckResponseFilter : IActionFilter
            {
                private readonly GlobalSettings _globalSettings;

                public UpdateCheckResponseFilter(IOptions<GlobalSettings> globalSettings)
                {
                    _globalSettings = globalSettings.Value;
                }

                public void OnActionExecuted(ActionExecutedContext context)
                {
                    if (context.HttpContext.Response == null) return;

                    if (context.Result is ObjectResult objectContent)
                    {
                        if (objectContent.Value == null) return;

                        context.HttpContext.Response.Cookies.Append("UMB_UPDCHK", "1", new CookieOptions()
                        {
                            Path = "/",
                            Expires = DateTimeOffset.Now.AddDays(_globalSettings.VersionCheckPeriod),
                            HttpOnly = true,
                            Secure = _globalSettings.UseHttps
                        });
                    }
                }

                public void OnActionExecuting(ActionExecutingContext context)
                {

                }
            }
        }

    }
}
