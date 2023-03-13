﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Profiling;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Profiling;

public class GetStatusProfilingController : ProfilingControllerBase
{
    private readonly IWebProfilerService _webProfilerService;

    public GetStatusProfilingController(IWebProfilerService webProfilerService) => _webProfilerService = webProfilerService;

    [HttpGet("status")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProfilingStatusResponseModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> Status()
    {
        Attempt<bool, WebProfilerOperationStatus> result = await _webProfilerService.GetStatus();
        return result.Success
            ? Ok(new ProfilingStatusResponseModel(result.Result))
            : WebProfilerOperationStatusResult(result.Status);
    }
}

