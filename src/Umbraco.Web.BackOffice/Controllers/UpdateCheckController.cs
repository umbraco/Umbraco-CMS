using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Semver;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.BackOffice.Controllers;

[PluginController(Constants.Web.Mvc.BackOfficeApiArea)]
public class UpdateCheckController : UmbracoAuthorizedJsonController
{
    private readonly IBackOfficeSecurityAccessor _backofficeSecurityAccessor;
    private readonly ICookieManager _cookieManager;
    private readonly GlobalSettings _globalSettings;
    private readonly IUmbracoVersion _umbracoVersion;
    private readonly IUpgradeService _upgradeService;

    public UpdateCheckController(
        IUpgradeService upgradeService,
        IUmbracoVersion umbracoVersion,
        ICookieManager cookieManager,
        IBackOfficeSecurityAccessor backofficeSecurityAccessor,
        IOptionsSnapshot<GlobalSettings> globalSettings)
    {
        _upgradeService = upgradeService ?? throw new ArgumentNullException(nameof(upgradeService));
        _umbracoVersion = umbracoVersion ?? throw new ArgumentNullException(nameof(umbracoVersion));
        _cookieManager = cookieManager ?? throw new ArgumentNullException(nameof(cookieManager));
        _backofficeSecurityAccessor = backofficeSecurityAccessor ??
                                      throw new ArgumentNullException(nameof(backofficeSecurityAccessor));
        _globalSettings = globalSettings.Value ?? throw new ArgumentNullException(nameof(globalSettings));
    }

    [UpdateCheckResponseFilter]
    public async Task<UpgradeCheckResponse?> GetCheck()
    {
        var updChkCookie = _cookieManager.GetCookieValue("UMB_UPDCHK");
        var updateCheckCookie = updChkCookie ?? string.Empty;
        if (_globalSettings.VersionCheckPeriod > 0 && string.IsNullOrEmpty(updateCheckCookie) &&
            (_backofficeSecurityAccessor.BackOfficeSecurity?.CurrentUser?.IsAdmin() ?? false))
        {
            try
            {
                var version = new SemVersion(_umbracoVersion.Version.Major, _umbracoVersion.Version.Minor,
                    _umbracoVersion.Version.Build, _umbracoVersion.Comment);
                UpgradeResult result = await _upgradeService.CheckUpgrade(version);

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
    ///     Adds the cookie response if it was successful
    /// </summary>
    /// <remarks>
    ///     A filter is required because we are returning an object from the get method and not an HttpResponseMessage
    /// </remarks>
    internal class UpdateCheckResponseFilterAttribute : TypeFilterAttribute
    {
        public UpdateCheckResponseFilterAttribute() : base(typeof(UpdateCheckResponseFilter))
        {
        }

        private class UpdateCheckResponseFilter : IActionFilter
        {
            private readonly GlobalSettings _globalSettings;

            public UpdateCheckResponseFilter(IOptionsSnapshot<GlobalSettings> globalSettings) =>
                _globalSettings = globalSettings.Value;

            public void OnActionExecuted(ActionExecutedContext context)
            {
                if (context.HttpContext.Response == null)
                {
                    return;
                }

                if (context.Result is ObjectResult objectContent)
                {
                    if (objectContent.Value == null)
                    {
                        return;
                    }

                    context.HttpContext.Response.Cookies.Append("UMB_UPDCHK", "1",
                        new CookieOptions
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
