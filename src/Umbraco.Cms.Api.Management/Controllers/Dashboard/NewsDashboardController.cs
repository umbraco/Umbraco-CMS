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

    public NewsDashboardController(INewsDashboardService newsDashboardService)
    {
        _newsDashboardService = newsDashboardService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<NewsDashboardResponseModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboard()
    {
        NewsDashboardResponseModel content = await _newsDashboardService.GetArticlesAsync();

        return Ok(content);
    }
}
