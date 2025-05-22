using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Api.Management.ViewModels.Server;

namespace Umbraco.Cms.Api.Management.Controllers.Server;

[ApiVersion("1.0")]
public class StatusServerController : ServerControllerBase
{
    private readonly IRuntimeState _runtimeState;

    public StatusServerController(IRuntimeState runtimeState) => _runtimeState = runtimeState;

    [AllowAnonymous]
    [HttpGet("status")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ServerStatusResponseModel), StatusCodes.Status200OK)]
    public Task<ActionResult<ServerStatusResponseModel>> Get(CancellationToken cancellationToken)
        => Task.FromResult<ActionResult<ServerStatusResponseModel>>(new ServerStatusResponseModel { ServerStatus = _runtimeState.Level });
}
