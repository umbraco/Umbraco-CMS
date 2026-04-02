using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Installer;
using Umbraco.Cms.Core.Services.Installer;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Upgrade;

/// <summary>
/// Provides endpoints for handling authorization requests during the Umbraco upgrade process.
/// </summary>
[ApiVersion("1.0")]
public class AuthorizeUpgradeController : UpgradeControllerBase
{
    private readonly IUpgradeService _upgradeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorizeUpgradeController"/> class with the specified upgrade service.
    /// </summary>
    /// <param name="upgradeService">The <see cref="IUpgradeService"/> to be used by the controller.</param>
    public AuthorizeUpgradeController(IUpgradeService upgradeService) => _upgradeService = upgradeService;

    [HttpPost("authorize")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status428PreconditionRequired)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Authorizes the upgrade.")]
    [EndpointDescription("Authorizes the currently authenticated user to perform the upgrade.")]
    public async Task<IActionResult> Authorize(CancellationToken cancellationToken)
    {
        Attempt<InstallationResult?, UpgradeOperationStatus> result = await _upgradeService.UpgradeAsync();
        return UpgradeOperationResult(result.Status, result.Result);
    }
}
