using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Infrastructure.Install;
using Umbraco.Cms.ManagementApi.ViewModels.Installer;
using Umbraco.New.Cms.Core.Factories;
using Umbraco.New.Cms.Core.Models.Installer;

namespace Umbraco.Cms.ManagementApi.Controllers.Install;

[ApiVersion("1.0")]
public class SettingsInstallController : InstallControllerBase
{
    private readonly InstallHelper _installHelper;
    private readonly IInstallSettingsFactory _installSettingsFactory;
    private readonly IUmbracoMapper _mapper;

    public SettingsInstallController(
        InstallHelper installHelper,
        IInstallSettingsFactory installSettingsFactory,
        IUmbracoMapper mapper)
    {
        _installHelper = installHelper;
        _installSettingsFactory = installSettingsFactory;
        _mapper = mapper;
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
}
