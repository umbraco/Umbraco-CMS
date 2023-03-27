using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.New.Cms.Core.Services.Installer;

namespace Umbraco.Cms.ManagementApi.Controllers.Upgrade;

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
    public async Task<IActionResult> Authorize()
    {
        await _upgradeService.Upgrade();
        return Ok();
    }
}
