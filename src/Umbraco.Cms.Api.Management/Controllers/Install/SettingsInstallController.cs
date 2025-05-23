using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Infrastructure.Install;
using Umbraco.Cms.Api.Management.ViewModels.Installer;
using Umbraco.Cms.Core.Factories;
using Umbraco.Cms.Core.Models.Installer;

namespace Umbraco.Cms.Api.Management.Controllers.Install;

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
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status428PreconditionRequired)]
    [ProducesResponseType(typeof(InstallSettingsResponseModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> Settings(CancellationToken cancellationToken)
    {
        // Register that the install has started
        await _installHelper.SetInstallStatusAsync(false, string.Empty);

        InstallSettingsModel installSettings = _installSettingsFactory.GetInstallSettings();
        InstallSettingsResponseModel responseModel = _mapper.Map<InstallSettingsResponseModel>(installSettings)!;

        return Ok(responseModel);
    }
}
