using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Infrastructure.ModelsBuilder;
using Umbraco.Cms.Api.Management.ViewModels.ModelsBuilderDashboard;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Controllers.ModelsBuilder;

/// <summary>
/// Controller that provides status information for the Models Builder.
/// </summary>
[ApiVersion("1.0")]
public class StatusModelsBuilderController : ModelsBuilderControllerBase
{
    private readonly OutOfDateModelsStatus _outOfDateModelsStatus;

    /// <summary>
    /// Initializes a new instance of the <see cref="StatusModelsBuilderController"/> class with the specified models status.
    /// </summary>
    /// <param name="outOfDateModelsStatus">An <see cref="OutOfDateModelsStatus"/> value indicating whether the generated models are out of date.</param>
    public StatusModelsBuilderController(OutOfDateModelsStatus outOfDateModelsStatus) => _outOfDateModelsStatus = outOfDateModelsStatus;

    /// <summary>
    /// Retrieves the current out-of-date status of the models builder, indicating whether the generated models are up to date with the underlying data schema.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>An <see cref="ActionResult{OutOfDateStatusResponseModel}"/> containing the status of the models builder: current, out-of-date, or unknown.</returns>
    [HttpGet("status")]
    [ProducesResponseType(typeof(OutOfDateStatusResponseModel), StatusCodes.Status200OK)]
    [EndpointSummary("Gets models builder status.")]
    [EndpointDescription("Gets the current status and configuration of the models builder.")]
    [MapToApiVersion("1.0")]
    public Task<ActionResult<OutOfDateStatusResponseModel>> GetModelsOutOfDateStatus(CancellationToken cancellationToken)
    {
        OutOfDateStatusResponseModel status = _outOfDateModelsStatus.IsEnabled
            ? _outOfDateModelsStatus.IsOutOfDate
                ? new OutOfDateStatusResponseModel { Status = OutOfDateType.OutOfDate }
                : new OutOfDateStatusResponseModel { Status = OutOfDateType.Current }
            : new OutOfDateStatusResponseModel { Status = OutOfDateType.Unknown };

        return Task.FromResult<ActionResult<OutOfDateStatusResponseModel>>(Ok(status));
    }
}
