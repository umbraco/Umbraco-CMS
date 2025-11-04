using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Services.NewsDashboard;
using Umbraco.Cms.Api.Management.ViewModels.NewsDashboard;

namespace Umbraco.Cms.Api.Management.Controllers.NewsDashboard;

public class NewsDashboardController : NewsDashboardControllerBase
{
    private readonly INewsDashboardService _newsDashboardService;

    public NewsDashboardController(INewsDashboardService newsDashboardService) => _newsDashboardService = newsDashboardService;

    [HttpGet]
    [ProducesResponseType(typeof(NewsDashboardResponseModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboard()
    {
        NewsDashboardResponseModel content = await _newsDashboardService.GetItemsAsync();

        return Ok(content);
    }
}
