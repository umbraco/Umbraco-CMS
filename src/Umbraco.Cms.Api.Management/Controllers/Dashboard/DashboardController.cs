using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Telemetry;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Dashboard;

[VersionedApiBackOfficeRoute("dashboard")]
[ApiExplorerSettings(GroupName = "Dashboard")]
public class DashboardController : ManagementApiControllerBase
{
    private readonly AppCaches _appCaches;
    private readonly ILogger<DashboardController> _logger;
    private readonly IUmbracoVersion _umbracoVersion;
    private readonly ISiteIdentifierService _siteIdentifierService;
    private static readonly HttpClient HttpClient = new();

    public DashboardController(AppCaches appCaches, ILogger<DashboardController> logger, IUmbracoVersion umbracoVersion, ISiteIdentifierService siteIdentifierService)
    {
        _appCaches = appCaches;
        _logger = logger;
        _umbracoVersion = umbracoVersion;
        _siteIdentifierService = siteIdentifierService;
    }

    [HttpGet]
    public async Task<IActionResult> GetDashboard()
    {
        var baseUrl = "https://umbraco-dashboard-news.euwest01.umbraco.io";
        var path = "/api/News";
        var key = "umbraco-dashboard-news";
        var version = _umbracoVersion.SemanticVersion.ToSemanticStringWithoutBuild();
        _siteIdentifierService.TryGetOrCreateSiteIdentifier(out Guid siteIdentifier);

        var url = $"{baseUrl}/{path}?version={version}&siteId={siteIdentifier}";

        JsonDocument? content = _appCaches.RuntimeCache.GetCacheItem<JsonDocument>(key);
        if (content is not null)
        {
            return Ok(content);
        }

        try
        {
            var json = await HttpClient.GetStringAsync(url);
            content = JsonDocument.Parse(json);
            _appCaches.RuntimeCache.InsertCacheItem(key, () => content, new TimeSpan(0, 30, 0));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.InnerException ?? ex, "Error getting dashboard content from {Url}", url);
            return NoContent();
        }


        return Ok(content);
    }
}
