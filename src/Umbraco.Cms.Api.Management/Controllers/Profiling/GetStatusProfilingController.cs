using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Profiling;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Profiling;

/// <summary>
/// Controller for retrieving the current status of profiling operations.
/// </summary>
[ApiVersion("1.0")]
public class GetStatusProfilingController : ProfilingControllerBase
{
    private readonly IWebProfilerService _webProfilerService;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetStatusProfilingController"/> class.
    /// </summary>
    /// <param name="webProfilerService">The <see cref="IWebProfilerService"/> instance used to provide profiling status information.</param>
    public GetStatusProfilingController(IWebProfilerService webProfilerService) => _webProfilerService = webProfilerService;

    /// <summary>
    /// Retrieves the current status of the MiniProfiler profiling tool.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> containing a <see cref="ProfilingStatusResponseModel"/> with the profiling status if successful;
    /// otherwise, an error result indicating the reason for failure.
    /// </returns>
    [HttpGet("status")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProfilingStatusResponseModel), StatusCodes.Status200OK)]
    [EndpointSummary("Gets profiling status.")]
    [EndpointDescription("Gets the current status of the MiniProfiler profiling tool.")]
    public async Task<IActionResult> Status(CancellationToken cancellationToken)
    {
        Attempt<bool, WebProfilerOperationStatus> result = await _webProfilerService.GetStatus();
        return result.Success
            ? Ok(new ProfilingStatusResponseModel(result.Result))
            : WebProfilerOperationStatusResult(result.Status);
    }
}

