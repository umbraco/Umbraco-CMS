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

namespace Umbraco.Cms.Api.Management.Controllers.NewsDashboard;

[VersionedApiBackOfficeRoute("news-dashboard")]
[ApiExplorerSettings(GroupName = "Dashboard")]
public class NewsDashboardController : ManagementApiControllerBase
{
    private readonly INewsDashboardService _newsDashboardService;
    private readonly ILogger<NewsDashboardController> _logger;
    private readonly IUmbracoVersion _umbracoVersion;
    private readonly ISiteIdentifierService _siteIdentifierService;

    public NewsDashboardController(INewsDashboardService newsDashboardService, ILogger<NewsDashboardController> logger, IUmbracoVersion umbracoVersion, ISiteIdentifierService siteIdentifierService)
    {
        _newsDashboardService = newsDashboardService;
        _logger = logger;
        _umbracoVersion = umbracoVersion;
        _siteIdentifierService = siteIdentifierService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<NewsDashboardItem>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboard()
    {
        IEnumerable<NewsDashboardItem> content = await _newsDashboardService.GetNewsItemsAsync();

        return Ok(content);
    }
}
