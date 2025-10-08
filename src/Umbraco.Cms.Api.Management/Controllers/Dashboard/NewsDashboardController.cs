using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Api.Management.Services.NewsDashboard;
using Umbraco.Cms.Api.Management.ViewModels.NewsDashboard;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Telemetry;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Dashboard;

[VersionedApiBackOfficeRoute("news-dashboard")]
[ApiExplorerSettings(GroupName = "Dashboard")]
public class NewsDashboardController : ManagementApiControllerBase
{
    private readonly AppCaches _appCaches;
    private readonly INewsDashboardService _newsDashboardService;
    private readonly ILogger<NewsDashboardController> _logger;
    private readonly IUmbracoVersion _umbracoVersion;
    private readonly ISiteIdentifierService _siteIdentifierService;
    private static readonly HttpClient HttpClient = new();

    public NewsDashboardController(AppCaches appCaches, INewsDashboardService newsDashboardService, ILogger<NewsDashboardController> logger, IUmbracoVersion umbracoVersion, ISiteIdentifierService siteIdentifierService)
    {
        _appCaches = appCaches;
        _newsDashboardService = newsDashboardService;
        _logger = logger;
        _umbracoVersion = umbracoVersion;
        _siteIdentifierService = siteIdentifierService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<NewsDashboardItem>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboard()
    {
        var baseUrl = "https://umbraco-dashboard-news.euwest01.umbraco.io";
        var path = "/api/News";
        var key = "umbraco-dashboard-news";
        var version = _umbracoVersion.SemanticVersion.ToSemanticStringWithoutBuild();
        _siteIdentifierService.TryGetOrCreateSiteIdentifier(out Guid siteIdentifier);

        var url = $"{baseUrl}/{path}?version={version}&siteId={siteIdentifier}";

        IEnumerable<NewsDashboardItem>? content = _appCaches.RuntimeCache.GetCacheItem<IEnumerable<NewsDashboardItem>?>(key);
        if (content is not null && content.Any())
        {
            return Ok(content);
        }

        try
        {
            var json = await HttpClient.GetStringAsync(url);

            if (_newsDashboardService.TryMapModel(json, out IEnumerable<NewsDashboardItem>? model))
            {
                _appCaches.RuntimeCache.InsertCacheItem(key, () => model, new TimeSpan(0, 30, 0));

                content = model;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.InnerException ?? ex, "Error getting dashboard content from {Url}", url);
            return NoContent();
        }

        return Ok(content);
    }
}
