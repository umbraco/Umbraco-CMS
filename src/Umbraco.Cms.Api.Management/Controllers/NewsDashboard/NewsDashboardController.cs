using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Services.NewsDashboard;
using Umbraco.Cms.Api.Management.ViewModels.NewsDashboard;

namespace Umbraco.Cms.Api.Management.Controllers.NewsDashboard;

    /// <summary>
    /// Provides API endpoints for managing and retrieving information for the News Dashboard in Umbraco CMS.
    /// </summary>
public class NewsDashboardController : NewsDashboardControllerBase
{
    private readonly INewsDashboardService _newsDashboardService;

    /// <summary>
    /// Initializes a new instance of the <see cref="NewsDashboardController"/> class.
    /// </summary>
    /// <param name="newsDashboardService">An instance of <see cref="INewsDashboardService"/> used to manage news dashboard operations.</param>
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
