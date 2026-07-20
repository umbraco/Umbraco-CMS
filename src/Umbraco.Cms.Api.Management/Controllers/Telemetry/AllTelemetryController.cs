using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.Telemetry;

namespace Umbraco.Cms.Api.Management.Controllers.Telemetry;

/// <summary>
/// Provides endpoints for accessing and managing telemetry data within the system.
/// </summary>
[ApiVersion("1.0")]
public class AllTelemetryController : TelemetryControllerBase
{
    /// <summary>
    /// Retrieves a paged list of telemetry levels and their associated statistics for the current Umbraco installation.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="skip">The number of telemetry levels to skip before starting to collect the results.</param>
    /// <param name="take">The maximum number of telemetry levels to return.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The result contains a <see cref="PagedViewModel{TelemetryResponseModel}"/> with the total count and a collection of telemetry response models for the requested telemetry levels.
    /// </returns>
    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<TelemetryResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets telemetry data.")]
    [EndpointDescription("Gets telemetry data and statistics for the Umbraco installation.")]
    public Task<PagedViewModel<TelemetryResponseModel>> GetAll(
        CancellationToken cancellationToken,
        int skip = 0,
        int take = 100)
    {
        TelemetryLevel[] levels = Enum.GetValues<TelemetryLevel>();
        return Task.FromResult(new PagedViewModel<TelemetryResponseModel>
        {
            Total = levels.Length,
            Items = levels.Skip(skip).Take(take).Select(level => new TelemetryResponseModel { TelemetryLevel = level }),
        });
    }
}
