using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.ManagementApi.Filters;
using Umbraco.Cms.ManagementApi.ViewModels.Installer;
using Umbraco.New.Cms.Core.Factories;
using Umbraco.New.Cms.Core.Models.Installer;
using Umbraco.New.Cms.Core.Services.Installer;

namespace Umbraco.Cms.ManagementApi.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/install")]
public class NewInstallController : Controller
{
    private readonly IUmbracoMapper _mapper;
    private readonly IInstallSettingsFactory _installSettingsFactory;
    private readonly IInstallService _installService;

    public NewInstallController(
        IUmbracoMapper mapper,
        IInstallSettingsFactory installSettingsFactory,
        IInstallService installService)
    {
        _mapper = mapper;
        _installSettingsFactory = installSettingsFactory;
        _installService = installService;
    }

    [HttpGet("settings")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status428PreconditionRequired)]
    [ProducesResponseType(typeof(InstallSettingsViewModel), StatusCodes.Status200OK)]
    [RequireRuntimeLevel(RuntimeLevel.Install)]
    public async Task<IActionResult> Settings()
    {
        InstallSettingsModel installSettings = _installSettingsFactory.GetInstallSettings();

        InstallSettingsViewModel viewModel = _mapper.Map<InstallSettingsViewModel>(installSettings)!;

        return Ok(viewModel);
    }

    [HttpPost("setup")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status428PreconditionRequired)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [MapToApiVersion("1.0")]
    [RequireRuntimeLevel(RuntimeLevel.Install)]
    public async Task<IActionResult> Setup(InstallViewModel installData)
    {
        if (ModelState.IsValid is false)
        {
            return BadRequest(ModelState);
        }

        InstallData data = _mapper.Map<InstallData>(installData)!;

        await _installService.Install(data);

        return Created("/", null);
    }

    [HttpPost("upgrade")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status428PreconditionRequired)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [RequireRuntimeLevel(RuntimeLevel.Upgrade)]
    public async Task<IActionResult> Upgrade()
    {
        await _installService.Upgrade();
        return Ok();
    }
}
