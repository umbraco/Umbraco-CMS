using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Server;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Server;

[ApiVersion("1.0")]
public class InformationServerController : ServerControllerBase
{
    private readonly IServerInformationService _serverInformationService;
    private readonly IUmbracoMapper _umbracoMapper;

    public InformationServerController(IServerInformationService serverInformationService, IUmbracoMapper umbracoMapper)
    {
        _serverInformationService = serverInformationService;
        _umbracoMapper = umbracoMapper;
    }

    [HttpGet("information")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ServerInformationResponseModel), StatusCodes.Status200OK)]
    public Task<IActionResult> Information(CancellationToken cancellationToken)
    {
        ServerInformationResponseModel responseModel = _umbracoMapper.Map<ServerInformationResponseModel>(_serverInformationService.GetServerInformation())!;

        return Task.FromResult<IActionResult>(Ok(responseModel));
    }
}
