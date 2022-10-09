using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Infrastructure.ModelsBuilder;
using Umbraco.Cms.ManagementApi.ViewModels.ModelsBuilderDashboard;
using Umbraco.New.Cms.Core.Models;

namespace Umbraco.Cms.ManagementApi.Controllers.ModelsBuilderDashboard;

public class StatusModelsBuilderController : ModelsBuilderControllerBase
{
    private readonly OutOfDateModelsStatus _outOfDateModelsStatus;

    public StatusModelsBuilderController(OutOfDateModelsStatus outOfDateModelsStatus) => _outOfDateModelsStatus = outOfDateModelsStatus;

    [HttpGet("status")]
    [ProducesResponseType(typeof(OutOfDateStatusViewModel), StatusCodes.Status200OK)]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<OutOfDateStatusViewModel>> GetModelsOutOfDateStatus()
    {
        OutOfDateStatusViewModel status = _outOfDateModelsStatus.IsEnabled
            ? _outOfDateModelsStatus.IsOutOfDate
                ? new OutOfDateStatusViewModel { Status = OutOfDateType.OutOfDate }
                : new OutOfDateStatusViewModel { Status = OutOfDateType.Current }
            : new OutOfDateStatusViewModel { Status = OutOfDateType.Unknown };

        return await Task.FromResult(Ok(status));
    }
}
