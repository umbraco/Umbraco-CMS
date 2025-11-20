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
    [EndpointSummary("Gets news dashboard content.")]
    [EndpointDescription("Gets the news dashboard content including recent news items and updates for the Umbraco back office.")]
    public async Task<IActionResult> GetDashboard()
    {
        NewsDashboardResponseModel content = await _newsDashboardService.GetItemsAsync();

        return Ok(content);
    }
}
