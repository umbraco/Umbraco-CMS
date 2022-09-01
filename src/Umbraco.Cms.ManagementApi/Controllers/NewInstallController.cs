using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Install.Models;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Infrastructure.Install;
using Umbraco.Cms.Infrastructure.Migrations.Install;
using Umbraco.Cms.ManagementApi.Builders;
using Umbraco.Cms.ManagementApi.Filters;
using Umbraco.Cms.ManagementApi.ViewModels.Installer;
using Umbraco.Extensions;
using Umbraco.New.Cms.Core.Factories;
using Umbraco.New.Cms.Core.Models.Installer;
using Umbraco.New.Cms.Core.Services.Installer;
using Umbraco.New.Cms.Web.Common.Routing;

namespace Umbraco.Cms.ManagementApi.Controllers;

[ApiController]
[ApiVersion("1.0")]
[BackOfficeRoute("api/v{version:apiVersion}/install")]
[RequireRuntimeLevel(RuntimeLevel.Install)]
public class NewInstallController : Controller
{
    private readonly IUmbracoMapper _mapper;
    private readonly IInstallSettingsFactory _installSettingsFactory;
    private readonly IInstallService _installService;
    private readonly GlobalSettings _globalSettings;
    private readonly IHostingEnvironment _hostingEnvironment;
    private readonly InstallHelper _installHelper;
    private readonly DatabaseBuilder _databaseBuilder;

    public NewInstallController(
        IUmbracoMapper mapper,
        IInstallSettingsFactory installSettingsFactory,
        IInstallService installService,
        IOptions<GlobalSettings> globalSettings,
        IHostingEnvironment hostingEnvironment,
        InstallHelper installHelper,
        DatabaseBuilder databaseBuilder)
    {
        _mapper = mapper;
        _installSettingsFactory = installSettingsFactory;
        _installService = installService;
        _globalSettings = globalSettings.Value;
        _hostingEnvironment = hostingEnvironment;
        _installHelper = installHelper;
        _databaseBuilder = databaseBuilder;
    }

    [HttpGet("settings")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status428PreconditionRequired)]
    [ProducesResponseType(typeof(InstallSettingsViewModel), StatusCodes.Status200OK)]
    public async Task<ActionResult<InstallSettingsViewModel>> Settings()
    {
        // Register that the install has started
        await _installHelper.SetInstallStatusAsync(false, string.Empty);

        InstallSettingsModel installSettings = _installSettingsFactory.GetInstallSettings();
        InstallSettingsViewModel viewModel = _mapper.Map<InstallSettingsViewModel>(installSettings)!;

        return viewModel;
    }

    [HttpPost("setup")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status428PreconditionRequired)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Setup(InstallViewModel installData)
    {
        InstallData data = _mapper.Map<InstallData>(installData)!;
        await _installService.Install(data);

        var backOfficePath = _globalSettings.GetBackOfficePath(_hostingEnvironment);
        return Created(backOfficePath, null);
    }

    [HttpPost("validateDatabase")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ValidateDatabase(DatabaseInstallViewModel viewModel)
    {
        // TODO: Async - We need to figure out what we want to do with async endpoints that doesn't do anything async
        // We want these to be async for future use (Ideally we'll have more async things),
        // But we need to figure out how we want to handle it in the meantime? use Task.FromResult or?
        DatabaseModel databaseModel = _mapper.Map<DatabaseModel>(viewModel)!;

        var success = _databaseBuilder.ConfigureDatabaseConnection(databaseModel, true);

        if (success)
        {
            return Ok();
        }

        ProblemDetails invalidModelProblem =
            new ProblemDetailsBuilder()
                .WithTitle("Invalid database configuration")
                .WithDetail("The provided database configuration is invalid")
                .WithStatus(StatusCodes.Status400BadRequest)
                .Build();

        return BadRequest(invalidModelProblem);
    }
}
