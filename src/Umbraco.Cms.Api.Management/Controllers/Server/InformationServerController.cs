using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Server;

[ApiVersion("1.0")]
public class InformationServerController : ServerControllerBase
{
    private readonly ISystemInformationService _systemInformationService;

    public InformationServerController(ISystemInformationService systemInformationService)
    {
        _systemInformationService = systemInformationService;
    }

    [HttpGet("information")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(Dictionary<string, object>), StatusCodes.Status200OK)]
    public Task<IActionResult> Information() => Task.FromResult<IActionResult>(Ok(_systemInformationService.GetServerInformation()));
}
