using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Infrastructure.Install;
using Umbraco.Cms.Api.Management.ViewModels.Installer;
using Umbraco.Cms.Core.Factories;
using Umbraco.Cms.Core.Models.Installer;

namespace Umbraco.Cms.Api.Management.Controllers.Install;

[ApiVersion("1.0")]
public class SettingsInstallController : InstallControllerBase
{
    private readonly IInstallSettingsFactory _installSettingsFactory;
    private readonly IUmbracoMapper _mapper;

    [Obsolete("Please use the constructor without the InstallHelper parameter. Scheduled for removal in Umbraco 19.")]
    public SettingsInstallController(
        InstallHelper installHelper,
        IInstallSettingsFactory installSettingsFactory,
        IUmbracoMapper mapper)
        : this(installSettingsFactory, mapper)
    {
    }

    [ActivatorUtilitiesConstructor]
    public SettingsInstallController(
        IInstallSettingsFactory installSettingsFactory,
        IUmbracoMapper mapper)
    {
        _installSettingsFactory = installSettingsFactory;
        _mapper = mapper;
    }

    [HttpGet("settings")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status428PreconditionRequired)]
    [ProducesResponseType(typeof(InstallSettingsResponseModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> Settings(CancellationToken cancellationToken)
    {
        InstallSettingsModel installSettings = _installSettingsFactory.GetInstallSettings();
        InstallSettingsResponseModel responseModel = _mapper.Map<InstallSettingsResponseModel>(installSettings)!;

        return Ok(responseModel);
    }
}
