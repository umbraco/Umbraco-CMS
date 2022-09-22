using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.ManagementApi.ViewModels.Pagination;

namespace Umbraco.Cms.ManagementApi.Controllers.Analytics;

public class AllAnalyticsController : AnalyticsControllerBase
{
    [HttpGet("all")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<TelemetryLevel>), StatusCodes.Status200OK)]
    public async Task<PagedViewModel<TelemetryLevel>> GetAll(int skip, int take)
    {
        TelemetryLevel[] levels = Enum.GetValues<TelemetryLevel>();
        return await Task.FromResult(new PagedViewModel<TelemetryLevel>
        {
            Total = levels.Length,
            Items = levels.Skip(skip).Take(take),
        });
    }
}
