using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Infrastructure.ModelsBuilder;
using Umbraco.Cms.ManagementApi.ViewModels.ModelsBuilderDashboard;
using Umbraco.New.Cms.Core.Models;

namespace Umbraco.Cms.ManagementApi.Controllers.ModelsBuilderDashboard;

public class StatusModelsBuilderDashboardController : ModelsBuilderDashboardControllerBase
{
    private readonly OutOfDateModelsStatus _outOfDateModelsStatus;

    public StatusModelsBuilderDashboardController(OutOfDateModelsStatus outOfDateModelsStatus) => _outOfDateModelsStatus = outOfDateModelsStatus;

    [HttpGet("status")]
    public ActionResult<OutOfDateStatusViewModel> GetModelsOutOfDateStatus()
    {
        OutOfDateStatusViewModel status = _outOfDateModelsStatus.IsEnabled
            ? _outOfDateModelsStatus.IsOutOfDate
                ? new OutOfDateStatusViewModel { Status = OutOfDateType.OutOfDate }
                : new OutOfDateStatusViewModel { Status = OutOfDateType.Current }
            : new OutOfDateStatusViewModel { Status = OutOfDateType.Unknown };

        return status;
    }
}
