using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Profiling;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Profiling;

public class GetStatusProfilingController : ProfilingControllerBase
{
    private readonly IWebProfilerService _webProfilerService;

    public GetStatusProfilingController(IWebProfilerService webProfilerService) => _webProfilerService = webProfilerService;

    [HttpGet("status")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProfilingStatusViewModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> Status()
    {
        var result = await _webProfilerService.GetStatus();
        return result.Success
            ? Ok(new ProfilingStatusViewModel(result.Result))
            : WebProfilerOperationStatusResult(result.Status);
    }
}

