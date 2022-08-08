using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.ManagementApi.Filters;
using Umbraco.Cms.ManagementApi.ViewModels.Installer;
using Umbraco.New.Cms.Core.Factories;
using Umbraco.New.Cms.Core.Models.Installer;
using Umbraco.New.Cms.Web.Common.Routing;

namespace Umbraco.Cms.ManagementApi.Controllers;

[ApiController]
[ApiVersion("1.0")]
[BackofficeRoute("api/v{version:apiVersion}/upgrade")]
public class UpgradeController : Controller
{
    private readonly IUpgradeSettingsFactory _upgradeSettingsFactory;
    private readonly IUmbracoMapper _mapper;

    public UpgradeController(
        IUpgradeSettingsFactory upgradeSettingsFactory,
        IUmbracoMapper mapper)
    {
        _upgradeSettingsFactory = upgradeSettingsFactory;
        _mapper = mapper;
    }

    [HttpGet("settings")]
    [MapToApiVersion("1.0")]
    [RequireRuntimeLevel(RuntimeLevel.Upgrade)]
    [ProducesResponseType(typeof(UpgradeSettingsViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status428PreconditionRequired)]
    public async Task<ActionResult<UpgradeSettingsViewModel>> Settings()
    {
        UpgradeSettingsModel upgradeSettings = _upgradeSettingsFactory.GetUpgradeSettings();
        UpgradeSettingsViewModel viewModel = _mapper.Map<UpgradeSettingsViewModel>(upgradeSettings)!;

        return viewModel;
    }
}
