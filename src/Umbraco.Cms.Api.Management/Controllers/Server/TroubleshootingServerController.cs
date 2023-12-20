using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Server;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Server;

[ApiVersion("1.0")]
public class TroubleshootingServerController : ServerControllerBase
{
    private readonly ISystemInformationService _systemInformationService;

<<<<<<<< HEAD:src/Umbraco.Cms.Api.Management/Controllers/Server/InformationServerController.cs
    public InformationServerController(ISystemInformationService systemInformationService)
========
    public TroubleshootingServerController(ISystemInformationService systemInformationService, IUmbracoMapper mapper)
>>>>>>>> origin/v14/feature/server-information:src/Umbraco.Cms.Api.Management/Controllers/Server/TroubleshootingServerController.cs
    {
        _systemInformationService = systemInformationService;
    }

    [HttpGet("troubleshooting")]
    [MapToApiVersion("1.0")]
<<<<<<<< HEAD:src/Umbraco.Cms.Api.Management/Controllers/Server/InformationServerController.cs
    [ProducesResponseType(typeof(ServerTroubleshootingResponseModel), StatusCodes.Status200OK)]
    public Task<IActionResult> Information() => Task.FromResult<IActionResult>(Ok(_systemInformationService.GetServerInformation()));
========
    [ProducesResponseType(typeof(ServerInformationResponseModel), StatusCodes.Status200OK)]
    public Task<IActionResult> GetTroubleshooting()
    {
        ServerInformationResponseModel responseModel = _mapper.Map<ServerInformationResponseModel>(_systemInformationService.GetTroubleshootingInformation())!;

        return Task.FromResult<IActionResult>(Ok(responseModel));
    }
>>>>>>>> origin/v14/feature/server-information:src/Umbraco.Cms.Api.Management/Controllers/Server/TroubleshootingServerController.cs
}
