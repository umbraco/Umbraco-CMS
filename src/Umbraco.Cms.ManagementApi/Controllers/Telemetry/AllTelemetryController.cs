using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.ManagementApi.ViewModels.Analytics;
using Umbraco.Cms.ManagementApi.ViewModels.Pagination;

namespace Umbraco.Cms.ManagementApi.Controllers.Telemetry;

public class AllTelemetryController : TelemetryControllerBase
{
    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<AnalyticsLevelViewModel>), StatusCodes.Status200OK)]
    public async Task<PagedViewModel<AnalyticsLevelViewModel>> GetAll(int skip, int take)
    {
        TelemetryLevel[] levels = Enum.GetValues<TelemetryLevel>();
        return await Task.FromResult(new PagedViewModel<AnalyticsLevelViewModel>
        {
            Total = levels.Length,
            Items = levels.Skip(skip).Take(take).Select(x => new AnalyticsLevelViewModel { AnalyticsLevel = x }),
        });
    }
}
