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
    private readonly ISystemInformationService _systemInformationService;
    private readonly IUmbracoMapper _mapper;

    public TroubleshootingServerController(ISystemInformationService systemInformationService, IUmbracoMapper mapper)
    {
        _systemInformationService = systemInformationService;
        _mapper = mapper;
    }

    [HttpGet("troubleshooting")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ServerTroubleshootingResponseModel), StatusCodes.Status200OK)]
    public Task<IActionResult> GetTroubleshooting()
    {
        ServerTroubleshootingResponseModel responseModel = _mapper.Map<ServerTroubleshootingResponseModel>(_systemInformationService.GetTroubleshootingInformation())!;

        return Task.FromResult<IActionResult>(Ok(responseModel));
    }
}
