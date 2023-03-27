using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.ViewModels.Server;

namespace Umbraco.Cms.ManagementApi.Controllers.Server;

[ApiVersion("1.0")]
public class StatusServerController : ServerControllerBase
{
    private readonly IRuntimeState _runtimeState;

    public StatusServerController(IRuntimeState runtimeState) => _runtimeState = runtimeState;

    [HttpGet("status")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ServerStatusViewModel), StatusCodes.Status200OK)]
    public async Task<ActionResult<ServerStatusViewModel>> Get() =>
        await Task.FromResult(new ServerStatusViewModel { ServerStatus = _runtimeState.Level });
}
