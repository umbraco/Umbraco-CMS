using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.Telemetry;

namespace Umbraco.Cms.Api.Management.Controllers.Telemetry;

public class AllTelemetryController : TelemetryControllerBase
{
    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<TelemetryViewModel>), StatusCodes.Status200OK)]
    public async Task<PagedViewModel<TelemetryViewModel>> GetAll(int skip, int take)
    {
        TelemetryLevel[] levels = Enum.GetValues<TelemetryLevel>();
        return await Task.FromResult(new PagedViewModel<TelemetryViewModel>
        {
            Total = levels.Length,
            Items = levels.Skip(skip).Take(take).Select(level => new TelemetryViewModel { TelemetryLevel = level }),
        });
    }
}
