using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Api.Management.ViewModels.Installer;
using Umbraco.Cms.Core.Factories;
using Umbraco.Cms.Core.Models.Installer;

namespace Umbraco.Cms.Api.Management.Controllers.Upgrade;

/// <summary>
/// API controller responsible for managing upgrade operations related to application settings.
/// </summary>
[ApiVersion("1.0")]
public class SettingsUpgradeController : UpgradeControllerBase
{
    private readonly IUpgradeSettingsFactory _upgradeSettingsFactory;
    private readonly IUmbracoMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsUpgradeController"/> class.
    /// </summary>
    /// <param name="upgradeSettingsFactory">A factory used to create upgrade settings.</param>
    /// <param name="mapper">An instance of <see cref="IUmbracoMapper"/> used for object mapping.</param>
    public SettingsUpgradeController(
        IUpgradeSettingsFactory upgradeSettingsFactory,
        IUmbracoMapper mapper)
    {
        _upgradeSettingsFactory = upgradeSettingsFactory;
        _mapper = mapper;
    }

    /// <summary>
    /// Retrieves the current upgrade settings and status for the Umbraco installation.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>
    /// An <see cref="ActionResult{T}"/> containing an <see cref="UpgradeSettingsResponseModel"/> with the current upgrade settings if successful (HTTP 200),
    /// or a <see cref="ProblemDetails"/> response if a precondition is required (HTTP 428).
    /// </returns>
    [HttpGet("settings")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(UpgradeSettingsResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status428PreconditionRequired)]
    [EndpointSummary("Gets upgrade settings.")]
    [EndpointDescription("Gets the current upgrade settings and status for the Umbraco installation.")]
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
