using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Server;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Server;

[ApiVersion("1.0")]
public class TroubleshootingServerController : ServerControllerBase
{
    private readonly ISystemTroubleshootingInformationService _systemTroubleshootingInformationService;
    private readonly IUmbracoMapper _mapper;

    public TroubleshootingServerController(ISystemTroubleshootingInformationService systemTroubleshootingInformationService, IUmbracoMapper mapper)
    {
        _systemTroubleshootingInformationService = systemTroubleshootingInformationService;
        _mapper = mapper;
    }

    [HttpGet("troubleshooting")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ServerTroubleshootingResponseModel), StatusCodes.Status200OK)]
    public Task<IActionResult> GetTroubleshooting(CancellationToken cancellationToken)
    {
        ServerTroubleshootingResponseModel responseModel = _mapper.Map<ServerTroubleshootingResponseModel>(_systemTroubleshootingInformationService.GetTroubleshootingInformation())!;

        return Task.FromResult<IActionResult>(Ok(responseModel));
    }
}
