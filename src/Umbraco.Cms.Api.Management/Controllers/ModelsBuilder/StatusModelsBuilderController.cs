using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Infrastructure.ModelsBuilder;
using Umbraco.Cms.Api.Management.ViewModels.ModelsBuilderDashboard;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Controllers.ModelsBuilder;

[ApiVersion("1.0")]
public class StatusModelsBuilderController : ModelsBuilderControllerBase
{
    private readonly OutOfDateModelsStatus _outOfDateModelsStatus;

    public StatusModelsBuilderController(OutOfDateModelsStatus outOfDateModelsStatus) => _outOfDateModelsStatus = outOfDateModelsStatus;

    [HttpGet("status")]
    [ProducesResponseType(typeof(OutOfDateStatusResponseModel), StatusCodes.Status200OK)]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<OutOfDateStatusResponseModel>> GetModelsOutOfDateStatus(CancellationToken cancellationToken)
    {
        OutOfDateStatusResponseModel status = _outOfDateModelsStatus.IsEnabled
            ? _outOfDateModelsStatus.IsOutOfDate
                ? new OutOfDateStatusResponseModel { Status = OutOfDateType.OutOfDate }
                : new OutOfDateStatusResponseModel { Status = OutOfDateType.Current }
            : new OutOfDateStatusResponseModel { Status = OutOfDateType.Unknown };

        return await Task.FromResult(Ok(status));
    }
}
