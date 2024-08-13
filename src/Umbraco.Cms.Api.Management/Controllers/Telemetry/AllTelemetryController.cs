using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.Telemetry;

namespace Umbraco.Cms.Api.Management.Controllers.Telemetry;

[ApiVersion("1.0")]
public class AllTelemetryController : TelemetryControllerBase
{
    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<TelemetryResponseModel>), StatusCodes.Status200OK)]
    public async Task<PagedViewModel<TelemetryResponseModel>> GetAll(
        CancellationToken cancellationToken,
        int skip = 0,
        int take = 100)
    {
        TelemetryLevel[] levels = Enum.GetValues<TelemetryLevel>();
        return await Task.FromResult(new PagedViewModel<TelemetryResponseModel>
        {
            Total = levels.Length,
            Items = levels.Skip(skip).Take(take).Select(level => new TelemetryResponseModel { TelemetryLevel = level }),
        });
    }
}
