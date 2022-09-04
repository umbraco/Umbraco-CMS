using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.ManagementApi.Filters;
using Umbraco.Cms.ManagementApi.ViewModels.Installer;
using Umbraco.New.Cms.Core.Factories;
using Umbraco.New.Cms.Core.Models.Installer;
using Umbraco.New.Cms.Core.Services.Installer;
using Umbraco.New.Cms.Web.Common.Routing;

namespace Umbraco.Cms.ManagementApi.Controllers;

// TODO: This needs to be an authorized controller.
[ApiController]
[ApiVersion("1.0")]
[RequireRuntimeLevel(RuntimeLevel.Upgrade)]
[VersionedApiBackOfficeRoute("upgrade")]
public class UpgradeController : Controller
{
    private readonly IUpgradeSettingsFactory _upgradeSettingsFactory;
    private readonly IUpgradeService _upgradeService;
    private readonly IUmbracoMapper _mapper;

    public UpgradeController(
        IUpgradeSettingsFactory upgradeSettingsFactory,
        IUpgradeService upgradeService,
        IUmbracoMapper mapper)
    {
        _upgradeSettingsFactory = upgradeSettingsFactory;
        _upgradeService = upgradeService;
        _mapper = mapper;
    }

    [HttpPost("authorize")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status428PreconditionRequired)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Authorize()
    {
        await _upgradeService.Upgrade();
        return Ok();
    }

    [HttpGet("settings")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(UpgradeSettingsViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status428PreconditionRequired)]
    public async Task<ActionResult<UpgradeSettingsViewModel>> Settings()
    {
        // TODO: Async - We need to figure out what we want to do with async endpoints that doesn't do anything async
        // We want these to be async for future use (Ideally we'll have more async things),
        // But we need to figure out how we want to handle it in the meantime? use Task.FromResult or?
        UpgradeSettingsModel upgradeSettings = _upgradeSettingsFactory.GetUpgradeSettings();
        UpgradeSettingsViewModel viewModel = _mapper.Map<UpgradeSettingsViewModel>(upgradeSettings)!;

        return viewModel;
    }
}
