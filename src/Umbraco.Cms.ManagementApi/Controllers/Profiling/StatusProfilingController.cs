using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.ManagementApi.ViewModels.Profiling;

namespace Umbraco.Cms.ManagementApi.Controllers.Profiling;

public class StatusProfilingController : ProfilingControllerBase
{
    private readonly IHostingEnvironment _hosting;

    public StatusProfilingController(IHostingEnvironment hosting) => _hosting = hosting;

    [HttpGet("status")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProfilingStatusViewModel), StatusCodes.Status200OK)]
    public async Task<ActionResult<ProfilingStatusViewModel>> Status()
        => await Task.FromResult(Ok(new ProfilingStatusViewModel(_hosting.IsDebugMode)));
}
