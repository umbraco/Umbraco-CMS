﻿using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Api.Management.ViewModels.Installer;
using Umbraco.Cms.Core.Models.Installer;
using Umbraco.Cms.Core.Services.Installer;

namespace Umbraco.Cms.Api.Management.Controllers.Install;

[ApiVersion("1.0")]
public class SetupInstallController : InstallControllerBase
{
    private readonly IUmbracoMapper _mapper;
    private readonly IInstallService _installService;

    public SetupInstallController(
        IUmbracoMapper mapper,
        IInstallService installService)
    {
        _mapper = mapper;
        _installService = installService;
    }

    [HttpPost("setup")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status428PreconditionRequired)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Setup(InstallRequestModel installData)
    {
        InstallData data = _mapper.Map<InstallData>(installData)!;
        await _installService.InstallAsync(data);

        return Ok();
    }
}
