using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Server;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Server;

/// <summary>
/// API controller that provides information about the server's status and configuration.
/// </summary>
[ApiVersion("1.0")]
public class InformationServerController : ServerControllerBase
{
    private readonly IServerInformationService _serverInformationService;
    private readonly IUmbracoMapper _umbracoMapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="InformationServerController"/> class, which provides server information endpoints for the Umbraco management API.
    /// </summary>
    /// <param name="serverInformationService">Service used to retrieve server information.</param>
    /// <param name="umbracoMapper">The mapper used to map Umbraco domain models to API models.</param>
    public InformationServerController(IServerInformationService serverInformationService, IUmbracoMapper umbracoMapper)
    {
        _serverInformationService = serverInformationService;
        _umbracoMapper = umbracoMapper;
    }

    /// <summary>
    /// Retrieves detailed information about the current server environment and configuration.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>An <see cref="IActionResult"/> containing a <see cref="ServerInformationResponseModel"/> with server details.</returns>
    [HttpGet("information")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ServerInformationResponseModel), StatusCodes.Status200OK)]
    [EndpointSummary("Gets server information.")]
    [EndpointDescription("Gets detailed information about the server environment and configuration.")]
    public Task<IActionResult> Information(CancellationToken cancellationToken)
    {
        ServerInformationResponseModel responseModel = _umbracoMapper.Map<ServerInformationResponseModel>(_serverInformationService.GetServerInformation())!;

        return Task.FromResult<IActionResult>(Ok(responseModel));
    }
}
