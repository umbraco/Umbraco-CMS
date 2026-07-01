using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Server;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Server;

/// <summary>
/// API controller responsible for handling and responding to server upgrade check requests.
/// Provides endpoints to determine if the server requires an upgrade or is up to date.
/// </summary>
[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.RequireAdminAccess)]
[Obsolete("Upgrade checks are no longer supported. Scheduled for removal in Umbraco 19.")]
public class UpgradeCheckServerController : ServerControllerBase
{
    private readonly IUpgradeService _upgradeService;
    private readonly IUmbracoVersion _umbracoVersion;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpgradeCheckServerController"/> class.
    /// </summary>
    /// <param name="upgradeService">Service responsible for checking available upgrades.</param>
    /// <param name="umbracoVersion">Provides information about the current Umbraco version.</param>
    public UpgradeCheckServerController(IUpgradeService upgradeService, IUmbracoVersion umbracoVersion)
    {
        _upgradeService = upgradeService;
        _umbracoVersion = umbracoVersion;
    }

    /// <summary>
    /// Checks if there are any available upgrades for the current Umbraco installation.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> containing an <see cref="UpgradeCheckResponseModel"/> with details about available upgrades, or an empty result if none are available.
    /// </returns>
    /// <remarks>
    /// This method is obsolete and scheduled for removal in Umbraco 19. Upgrade checks are no longer supported.
    /// </remarks>
    [HttpGet("upgrade-check")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(UpgradeCheckResponseModel), StatusCodes.Status200OK)]
    [Obsolete("Upgrade checks are no longer supported. Scheduled for removal in Umbraco 19.")]
    [EndpointSummary("Checks for available upgrades.")]
    [EndpointDescription("Checks if there are any available upgrades for the Umbraco installation.")]
    public async Task<IActionResult> UpgradeCheck(CancellationToken cancellationToken)
    {
        UpgradeResult upgradeResult = await _upgradeService.CheckUpgrade(_umbracoVersion.SemanticVersion);

        var responseModel = new UpgradeCheckResponseModel
        {
            Type = upgradeResult.UpgradeType,
            Comment = upgradeResult.Comment,
            Url = upgradeResult.UpgradeUrl.IsNullOrWhiteSpace()
                ? string.Empty
                : $"{upgradeResult.UpgradeUrl}?version={_umbracoVersion.Version.ToString(3)}"
        };

        return Ok(responseModel);
    }
}
