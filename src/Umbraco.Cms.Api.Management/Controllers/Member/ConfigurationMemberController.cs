using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Member;

namespace Umbraco.Cms.Api.Management.Controllers.Member;

/// <summary>
/// Provides API endpoints for managing member configuration settings in the Umbraco CMS.
/// </summary>
[ApiVersion("1.0")]
public class ConfigurationMemberController : MemberControllerBase
{
    private readonly IConfigurationPresentationFactory _configurationPresentationFactory;

    public ConfigurationMemberController(
        IConfigurationPresentationFactory configurationPresentationFactory) =>
        _configurationPresentationFactory = configurationPresentationFactory;

    /// <summary>
    /// Retrieves the configuration settings for members.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>An <see cref="IActionResult"/> containing a <see cref="MemberConfigurationResponseModel"/> with the member configuration settings.</returns>
    [HttpGet("configuration")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(MemberConfigurationResponseModel), StatusCodes.Status200OK)]
    [EndpointSummary("Gets the member configuration.")]
    [EndpointDescription("Gets the configuration settings for members.")]
    public Task<IActionResult> Configuration(CancellationToken cancellationToken)
    {
        MemberConfigurationResponseModel responseModel = _configurationPresentationFactory.CreateMemberConfigurationResponseModel();
        return Task.FromResult<IActionResult>(Ok(responseModel));
    }
}
