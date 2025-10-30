using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Umbraco.Cms.Api.Management.ViewModels.ModelsBuilderDashboard;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.ModelsBuilder;

namespace Umbraco.Cms.Api.Management.Controllers.ModelsBuilder;

[ApiVersion("1.0")]
public class StatusModelsBuilderController : ModelsBuilderControllerBase
{
    private readonly OutOfDateModelsStatus _outOfDateModelsStatus;

    public StatusModelsBuilderController(OutOfDateModelsStatus outOfDateModelsStatus) => _outOfDateModelsStatus = outOfDateModelsStatus;

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
