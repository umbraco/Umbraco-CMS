using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Profiling;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Profiling;

public class UpdateStatusProfilingController : ProfilingControllerBase
{
    private readonly IWebProfilerService _webProfilerService;

    public UpdateStatusProfilingController(IWebProfilerService webProfilerService) => _webProfilerService = webProfilerService;

    [HttpPut("status")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Status(ProfilingStatusViewModel model)
    {
        var result = await _webProfilerService.SetStatus(model.Enabled);
        return result.Success
            ? Ok()
            : WebProfilerOperationStatusResult(result.Status);
    }
}

