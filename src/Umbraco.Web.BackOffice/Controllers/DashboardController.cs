using System.Net;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Dashboards;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Core.Telemetry;
using Umbraco.Cms.Web.BackOffice.Filters;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Web.Common.Filters;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.BackOffice.Controllers;

//we need to fire up the controller like this to enable loading of remote css directly from this controller
[PluginController(Constants.Web.Mvc.BackOfficeApiArea)]
[ValidationFilter]
[AngularJsonOnlyConfiguration] // TODO: This could be applied with our Application Model conventions
[IsBackOffice]
[Authorize(Policy = AuthorizationPolicies.BackOfficeAccess)]
public class DashboardController : UmbracoApiController
{
    //we have just one instance of HttpClient shared for the entire application
    private static readonly HttpClient HttpClient = new();
    private readonly AppCaches _appCaches;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IDashboardService _dashboardService;
    private readonly ContentDashboardSettings _dashboardSettings;
    private readonly ILogger<DashboardController> _logger;
    private readonly IShortStringHelper _shortStringHelper;
    private readonly ISiteIdentifierService _siteIdentifierService;
    private readonly IUmbracoVersion _umbracoVersion;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DashboardController" /> with all its dependencies.
    /// </summary>
    [ActivatorUtilitiesConstructor]
    public DashboardController(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        AppCaches appCaches,
        ILogger<DashboardController> logger,
        IDashboardService dashboardService,
        IUmbracoVersion umbracoVersion,
        IShortStringHelper shortStringHelper,
        IOptionsSnapshot<ContentDashboardSettings> dashboardSettings,
        ISiteIdentifierService siteIdentifierService)

    {
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _appCaches = appCaches;
        _logger = logger;
        _dashboardService = dashboardService;
        _umbracoVersion = umbracoVersion;
        _shortStringHelper = shortStringHelper;
        _siteIdentifierService = siteIdentifierService;
        _dashboardSettings = dashboardSettings.Value;
    }

    // TODO(V10) : change return type to Task<ActionResult<JObject>> and consider removing baseUrl as parameter
    //we have baseurl as a param to make previewing easier, so we can test with a dev domain from client side
    [ValidateAngularAntiForgeryToken]
    public async Task<JObject> GetRemoteDashboardContent(string section, string? baseUrl)
    {
        if (baseUrl is null)
        {
            baseUrl = "https://dashboard.umbraco.com/";
        }

        IUser? user = _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser;
        var allowedSections = string.Join(",", user?.AllowedSections ?? Array.Empty<string>());
        var language = user?.Language;
        var version = _umbracoVersion.SemanticVersion.ToSemanticStringWithoutBuild();
        var isAdmin = user?.IsAdmin() ?? false;
        _siteIdentifierService.TryGetOrCreateSiteIdentifier(out Guid siteIdentifier);

        if (!IsAllowedUrl(baseUrl))
        {
            _logger.LogError(
                $"The following URL is not listed in the setting 'Umbraco:CMS:ContentDashboard:ContentDashboardUrlAllowlist' in configuration: {baseUrl}");
            HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;

            // Hacking the response - can't set the HttpContext.Response.Body, so instead returning the error as JSON
            var errorJson = JsonConvert.SerializeObject(new { Error = "Dashboard source not permitted" });
            return JObject.Parse(errorJson);
        }

        var url = string.Format(
            "{0}{1}?section={2}&allowed={3}&lang={4}&version={5}&admin={6}&siteid={7}",
            baseUrl,
            _dashboardSettings.ContentDashboardPath,
            section,
            allowedSections,
            language,
            version,
            isAdmin,
            siteIdentifier);
        var key = "umbraco-dynamic-dashboard-" + language + allowedSections.Replace(",", "-") + section;

        JObject? content = _appCaches.RuntimeCache.GetCacheItem<JObject>(key);
        var result = new JObject();
        if (content != null)
        {
            result = content;
        }
        else
        {
            //content is null, go get it
            try
            {
                //fetch dashboard json and parse to JObject
                var json = await HttpClient.GetStringAsync(url);
                content = JObject.Parse(json);
                result = content;

                _appCaches.RuntimeCache.InsertCacheItem(key, () => result, new TimeSpan(0, 30, 0));
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex.InnerException ?? ex, "Error getting dashboard content from {Url}", url);

                //it's still new JObject() - we return it like this to avoid error codes which triggers UI warnings
                _appCaches.RuntimeCache.InsertCacheItem(key, () => result, new TimeSpan(0, 5, 0));
            }
        }

        return result;
    }

    // TODO(V10) : consider removing baseUrl as parameter
    public async Task<IActionResult> GetRemoteDashboardCss(string section, string? baseUrl)
    {
        if (baseUrl is null)
        {
            baseUrl = "https://dashboard.umbraco.org/";
        }

        if (!IsAllowedUrl(baseUrl))
        {
            _logger.LogError(
                $"The following URL is not listed in the setting 'Umbraco:CMS:ContentDashboard:ContentDashboardUrlAllowlist' in configuration: {baseUrl}");
            return BadRequest("Dashboard source not permitted");
        }

        var url = string.Format(baseUrl + "css/dashboard.css?section={0}", section);
        var key = "umbraco-dynamic-dashboard-css-" + section;

        var content = _appCaches.RuntimeCache.GetCacheItem<string>(key);
        var result = string.Empty;

        if (content != null)
        {
            result = content;
        }
        else
        {
            //content is null, go get it
            try
            {
                //fetch remote css
                content = await HttpClient.GetStringAsync(url);

                //can't use content directly, modified closure problem
                result = content;

                //save server content for 30 mins
                _appCaches.RuntimeCache.InsertCacheItem(key, () => result, new TimeSpan(0, 30, 0));
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex.InnerException ?? ex, "Error getting dashboard CSS from {Url}", url);

                //it's still string.Empty - we return it like this to avoid error codes which triggers UI warnings
                _appCaches.RuntimeCache.InsertCacheItem(key, () => result, new TimeSpan(0, 5, 0));
            }
        }


        return Content(result, "text/css", Encoding.UTF8);
    }

    public async Task<IActionResult> GetRemoteXml(string site, string url)
    {
        if (!IsAllowedUrl(url))
        {
            _logger.LogError(
                $"The following URL is not listed in the setting 'Umbraco:CMS:ContentDashboard:ContentDashboardUrlAllowlist' in configuration: {url}");
            return BadRequest("Dashboard source not permitted");
        }

        // This is used in place of the old feedproxy.config
        // Which was used to grab data from our.umbraco.com, umbraco.com or umbraco.tv
        // for certain dashboards or the help drawer
        var urlPrefix = string.Empty;
        switch (site.ToUpper())
        {
            case "TV":
                urlPrefix = "https://umbraco.tv/";
                break;

            case "OUR":
                urlPrefix = "https://our.umbraco.com/";
                break;

            case "COM":
                urlPrefix = "https://umbraco.com/";
                break;

            default:
                return NotFound();
        }


        //Make remote call to fetch videos or remote dashboard feed data
        var key = $"umbraco-XML-feed-{site}-{url.ToCleanString(_shortStringHelper, CleanStringType.UrlSegment)}";

        var content = _appCaches.RuntimeCache.GetCacheItem<string>(key);
        var result = string.Empty;

        if (content != null)
        {
            result = content;
        }
        else
        {
            //content is null, go get it
            try
            {
                //fetch remote css
                content = await HttpClient.GetStringAsync($"{urlPrefix}{url}");

                //can't use content directly, modified closure problem
                result = content;

                //save server content for 30 mins
                _appCaches.RuntimeCache.InsertCacheItem(key, () => result, new TimeSpan(0, 30, 0));
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex.InnerException ?? ex, "Error getting remote dashboard data from {UrlPrefix}{Url}", urlPrefix, url);

                //it's still string.Empty - we return it like this to avoid error codes which triggers UI warnings
                _appCaches.RuntimeCache.InsertCacheItem(key, () => result, new TimeSpan(0, 5, 0));
            }
        }

        return Content(result, "text/xml", Encoding.UTF8);
    }

    // return IDashboardSlim - we don't need sections nor access rules
    [ValidateAngularAntiForgeryToken]
    [OutgoingEditorModelEvent]
    public IEnumerable<Tab<IDashboardSlim>> GetDashboard(string section)
    {
        IUser? currentUser = _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser;
        return _dashboardService.GetDashboards(section, currentUser).Select(x => new Tab<IDashboardSlim>
        {
            Id = x.Id,
            Key = x.Key,
            Label = x.Label,
            Alias = x.Alias,
            Type = x.Type,
            Expanded = x.Expanded,
            IsActive = x.IsActive,
            Properties = x.Properties?.Select(y => new DashboardSlim { Alias = y.Alias, View = y.View })
        }).ToList();
    }

    // Checks if the passed URL is part of the configured allowlist of addresses
    private bool IsAllowedUrl(string url)
    {
        // No addresses specified indicates that any URL is allowed
        if (_dashboardSettings.ContentDashboardUrlAllowlist is null ||
            _dashboardSettings.ContentDashboardUrlAllowlist.Contains(url, StringComparer.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }
}
