using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Profiling;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Profiling;

/// <summary>
/// Controller responsible for managing profiling operations related to update status in the system.
/// </summary>
[ApiVersion("1.0")]
public class UpdateStatusProfilingController : ProfilingControllerBase
{
    private readonly IWebProfilerService _webProfilerService;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateStatusProfilingController"/> class, which manages profiling status updates.
    /// </summary>
    /// <param name="webProfilerService">The service used to handle web profiling operations.</param>
    public UpdateStatusProfilingController(IWebProfilerService webProfilerService) => _webProfilerService = webProfilerService;

    /// <summary>
    /// Updates the status of web profiling by enabling or disabling it based on the provided request model.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="model">The model containing the desired profiling status.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the outcome of the operation.</returns>
    [HttpPut("status")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [EndpointSummary("Updates the web profiling status.")]
    [EndpointDescription("Enables or disables web profiling according to the values provided in the request model.")]
    public async Task<IActionResult> Status(
        CancellationToken cancellationToken,
        ProfilingStatusRequestModel model)
    {
        Attempt<bool, WebProfilerOperationStatus> result = await _webProfilerService.SetStatus(model.Enabled);
        return result.Success
            ? Ok()
            : WebProfilerOperationStatusResult(result.Status);
    }
}

