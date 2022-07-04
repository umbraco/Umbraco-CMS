using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.BackOfficeApi.Factories.Installer;
using Umbraco.Cms.BackOfficeApi.Models.Installer;
using Umbraco.Cms.BackOfficeApi.Services;
using Umbraco.Cms.BackOfficeApi.ViewModels.Installer;
using Umbraco.Cms.Core.Install.Models;
using Umbraco.Cms.Core.Install.NewInstallSteps;
using Umbraco.Cms.Core.Install.NewModels;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.BackOfficeApi.Controllers;

// TODO: Create a filter to require install/upgrade for endpoints to be valid

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/install")]
public class NewInstallController : Controller
{
    private readonly IEnumerable<NewInstallSetupStep> _steps;
    private readonly ILogger<NewInstallController> _logger;
    private readonly IRuntime _runtime;
    private readonly IUmbracoMapper _mapper;
    private readonly IInstallSettingsFactory _installSettingsFactory;
    private readonly IInstallService _installService;

    public NewInstallController(
        IEnumerable<NewInstallSetupStep> steps,
        ILogger<NewInstallController> logger,
        IRuntime runtime,
        IUmbracoMapper mapper,
        IInstallSettingsFactory installSettingsFactory,
        IInstallService installService)
    {
        _steps = steps;
        _logger = logger;
        _runtime = runtime;
        _mapper = mapper;
        _installSettingsFactory = installSettingsFactory;
        _installService = installService;
    }

    [HttpGet("settings")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(InstallSettingsViewModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> Settings()
    {
        InstallSettingsModel installSettings = _installSettingsFactory.GetInstallSettings();

        InstallSettingsViewModel viewModel = _mapper.Map<InstallSettingsViewModel>(installSettings)!;

        return Ok(viewModel);
    }

    [HttpPost("setup")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [MapToApiVersion("1.0")]
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
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Upgrade()
    {
        // TODO: Do this correctly, this is just here to test NewDatabaseUpgradeStep
        var model = new InstallData();
        IOrderedEnumerable<NewInstallSetupStep> orderedSteps = _steps
            .Where(x => x.InstallationTypeTarget.HasFlag(InstallationType.Upgrade))
            .OrderBy(x => x.Order);

        foreach (NewInstallSetupStep step in orderedSteps)
        {
            var stepName = step.Name;
            _logger.LogInformation("Checking if {StepName} requires execution", stepName);
            if (await step.RequiresExecutionAsync(model) is false)
            {
                _logger.LogInformation("Skipping {StepName}", stepName);
                continue;
            }

            _logger.LogInformation("Running {StepName}", stepName);
            await step.ExecuteAsync(model);
        }

        // Restart the runtime now that the installer has run
        await _runtime.RestartAsync();
        return Ok();
    }
}
