using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Installer;
using Umbraco.Cms.Core.Services.Installer;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Upgrade;

[ApiVersion("1.0")]
public class AuthorizeUpgradeController : UpgradeControllerBase
{
    private readonly IUpgradeService _upgradeService;

    public AuthorizeUpgradeController(IUpgradeService upgradeService) => _upgradeService = upgradeService;

    [HttpPost("authorize")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status428PreconditionRequired)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Authorize(CancellationToken cancellationToken)
    {
        Attempt<InstallationResult?, UpgradeOperationStatus> result = await _upgradeService.UpgradeAsync();
        return UpgradeOperationResult(result.Status, result.Result);
    }
}
