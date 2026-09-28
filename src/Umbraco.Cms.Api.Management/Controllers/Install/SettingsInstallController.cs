using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Api.Management.ViewModels.Installer;
using Umbraco.Cms.Core.Factories;
using Umbraco.Cms.Core.Models.Installer;

namespace Umbraco.Cms.Api.Management.Controllers.Install;

/// <summary>
/// API controller responsible for handling and managing installation settings within the Umbraco CMS management interface.
/// </summary>
[ApiVersion("1.0")]
public class SettingsInstallController : InstallControllerBase
{
    private readonly IInstallSettingsFactory _installSettingsFactory;
    private readonly IUmbracoMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsInstallController"/> class.
    /// </summary>
    /// <param name="installSettingsFactory">An instance of <see cref="IInstallSettingsFactory"/> used to provide install settings.</param>
    /// <param name="mapper">An <see cref="IUmbracoMapper"/> used for object mapping.</param>
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
    [EndpointSummary("Gets install settings.")]
    [EndpointDescription("Gets the current installation settings and status.")]
    public async Task<IActionResult> Settings(CancellationToken cancellationToken)
    {
        InstallSettingsModel installSettings = _installSettingsFactory.GetInstallSettings();
        InstallSettingsResponseModel responseModel = _mapper.Map<InstallSettingsResponseModel>(installSettings)!;

        return Ok(responseModel);
    }
}
