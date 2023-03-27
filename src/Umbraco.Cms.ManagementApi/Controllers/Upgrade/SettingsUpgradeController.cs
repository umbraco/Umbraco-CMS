using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.ManagementApi.ViewModels.Installer;
using Umbraco.New.Cms.Core.Factories;
using Umbraco.New.Cms.Core.Models.Installer;

namespace Umbraco.Cms.ManagementApi.Controllers.Upgrade;

[ApiVersion("1.0")]
public class SettingsUpgradeController : UpgradeControllerBase
{
    private readonly IUpgradeSettingsFactory _upgradeSettingsFactory;
    private readonly IUmbracoMapper _mapper;

    public SettingsUpgradeController(
        IUpgradeSettingsFactory upgradeSettingsFactory,
        IUmbracoMapper mapper)
    {
        _upgradeSettingsFactory = upgradeSettingsFactory;
        _mapper = mapper;
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

        return await Task.FromResult(viewModel);
    }
}
