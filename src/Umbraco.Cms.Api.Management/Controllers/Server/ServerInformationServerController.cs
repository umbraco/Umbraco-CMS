using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Server;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Server;

[ApiVersion("1.0")]
public class ServerInformationServerController : ServerControllerBase
{
    private readonly ISystemInformationService _systemInformationService;
    private readonly IUmbracoMapper _mapper;

    public ServerInformationServerController(ISystemInformationService systemInformationService, IUmbracoMapper mapper)
    {
        _systemInformationService = systemInformationService;
        _mapper = mapper;
    }

    [HttpGet("information")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ServerInformationResponseModel), StatusCodes.Status200OK)]
    public Task<IActionResult> GetServerInformation()
    {
        ServerInformationResponseModel responseModel = _mapper.Map<ServerInformationResponseModel>(_systemInformationService.GetSystemInformation())!;

        return Task.FromResult<IActionResult>(Ok(responseModel));
    }
}
