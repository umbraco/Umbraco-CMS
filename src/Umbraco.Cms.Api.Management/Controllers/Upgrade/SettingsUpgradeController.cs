using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Api.Management.ViewModels.Installer;
using Umbraco.Cms.Core.Factories;
using Umbraco.Cms.Core.Models.Installer;

namespace Umbraco.Cms.Api.Management.Controllers.Upgrade;

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
    [ProducesResponseType(typeof(UpgradeSettingsResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status428PreconditionRequired)]
    public Task<ActionResult<UpgradeSettingsResponseModel>> Settings(CancellationToken cancellationToken)
    {
        // TODO: Async - We need to figure out what we want to do with async endpoints that doesn't do anything async
        // We want these to be async for future use (Ideally we'll have more async things),
        // But we need to figure out how we want to handle it in the meantime? use Task.FromResult or?
        UpgradeSettingsModel upgradeSettings = _upgradeSettingsFactory.GetUpgradeSettings();
        UpgradeSettingsResponseModel responseModel = _mapper.Map<UpgradeSettingsResponseModel>(upgradeSettings)!;

        return Task.FromResult<ActionResult<UpgradeSettingsResponseModel>>(responseModel);
    }
}
