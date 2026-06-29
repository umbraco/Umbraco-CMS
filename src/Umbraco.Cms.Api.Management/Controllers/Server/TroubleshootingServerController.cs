using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Server;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Server;

/// <summary>
/// Provides API endpoints for troubleshooting server-related issues in the management interface.
/// </summary>
[ApiVersion("1.0")]
public class TroubleshootingServerController : ServerControllerBase
{
    private readonly ISystemTroubleshootingInformationService _systemTroubleshootingInformationService;
    private readonly IUmbracoMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="TroubleshootingServerController"/> class with the specified troubleshooting information service and mapper.
    /// </summary>
    /// <param name="systemTroubleshootingInformationService">Service used to retrieve system troubleshooting information.</param>
    /// <param name="mapper">The <see cref="IUmbracoMapper"/> instance used for mapping models.</param>
    public TroubleshootingServerController(ISystemTroubleshootingInformationService systemTroubleshootingInformationService, IUmbracoMapper mapper)
    {
        _systemTroubleshootingInformationService = systemTroubleshootingInformationService;
        _mapper = mapper;
    }

    /// <summary>
    /// Retrieves troubleshooting and diagnostic information for the server.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> containing a <see cref="ServerTroubleshootingResponseModel"/> with server troubleshooting information.
    /// </returns>
    [HttpGet("troubleshooting")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ServerTroubleshootingResponseModel), StatusCodes.Status200OK)]
    [EndpointSummary("Gets server troubleshooting information.")]
    [EndpointDescription("Gets troubleshooting information and diagnostics for the server.")]
    public Task<IActionResult> GetTroubleshooting(CancellationToken cancellationToken)
    {
        ServerTroubleshootingResponseModel responseModel = _mapper.Map<ServerTroubleshootingResponseModel>(_systemTroubleshootingInformationService.GetTroubleshootingInformation())!;

        return Task.FromResult<IActionResult>(Ok(responseModel));
    }
}
