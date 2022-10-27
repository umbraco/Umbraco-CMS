using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.ManagementApi.ViewModels.Pagination;
using Umbraco.Cms.ManagementApi.ViewModels.Telemetry;

namespace Umbraco.Cms.ManagementApi.Controllers.Telemetry;

public class AllTelemetryController : TelemetryControllerBase
{
    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<TelemetryLevelViewModel>), StatusCodes.Status200OK)]
    public async Task<PagedViewModel<TelemetryLevelViewModel>> GetAll(int skip, int take)
    {
        TelemetryLevel[] levels = Enum.GetValues<TelemetryLevel>();
        return await Task.FromResult(new PagedViewModel<TelemetryLevelViewModel>
        {
            Total = levels.Length,
            Items = levels.Skip(skip).Take(take).Select(level => new TelemetryLevelViewModel { TelemetryLevel = level }),
        });
    }
}
