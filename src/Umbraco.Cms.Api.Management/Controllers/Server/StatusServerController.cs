using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Api.Management.ViewModels.Server;

namespace Umbraco.Cms.Api.Management.Controllers.Server;

    /// <summary>
    /// Provides endpoints for retrieving server status and health information.
    /// </summary>
[ApiVersion("1.0")]
public class StatusServerController : ServerControllerBase
{
    private readonly IRuntimeState _runtimeState;

    /// <summary>
    /// Initializes a new instance of the <see cref="StatusServerController"/> class, using the specified runtime state.
    /// </summary>
    /// <param name="runtimeState">An <see cref="IRuntimeState"/> instance representing the current runtime state of the application.</param>
    public StatusServerController(IRuntimeState runtimeState) => _runtimeState = runtimeState;

    /// <summary>
    /// Retrieves the current operational status of the Umbraco server.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>An <see cref="ActionResult{T}"/> containing a <see cref="ServerStatusResponseModel"/> with the server's status.</returns>
    [AllowAnonymous]
    [HttpGet("status")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ServerStatusResponseModel), StatusCodes.Status200OK)]
    [EndpointSummary("Gets server status.")]
    [EndpointDescription("Gets the current operational status of the Umbraco server.")]
    public Task<ActionResult<ServerStatusResponseModel>> Get(CancellationToken cancellationToken)
        => Task.FromResult<ActionResult<ServerStatusResponseModel>>(new ServerStatusResponseModel { ServerStatus = _runtimeState.Level });
}
