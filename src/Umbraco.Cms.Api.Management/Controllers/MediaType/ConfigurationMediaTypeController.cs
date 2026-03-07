using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.MediaType;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.MediaType;

    /// <summary>
    /// Provides API endpoints for managing media type configurations.
    /// </summary>
[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessMediaTypes)]
public class ConfigurationMediaTypeController : MediaTypeControllerBase
{
    private readonly IConfigurationPresentationFactory _configurationPresentationFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationMediaTypeController"/> class.
    /// </summary>
    /// <param name="configurationPresentationFactory">
    /// An instance of <see cref="IConfigurationPresentationFactory"/> used to create configuration presentations for media types.
    /// </param>
    public ConfigurationMediaTypeController(IConfigurationPresentationFactory configurationPresentationFactory)
    {
        _configurationPresentationFactory = configurationPresentationFactory;
    }

    /// <summary>
    /// Retrieves the configuration settings for media types.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>An <see cref="IActionResult"/> containing a <see cref="MediaTypeConfigurationResponseModel"/> with the current media type configuration.</returns>
    [HttpGet("configuration")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(MediaTypeConfigurationResponseModel), StatusCodes.Status200OK)]
    [EndpointSummary("Gets the media type configuration.")]
    [EndpointDescription("Gets the configuration settings for media types.")]
    public Task<IActionResult> Configuration(CancellationToken cancellationToken)
    {
        MediaTypeConfigurationResponseModel responseModel = _configurationPresentationFactory.CreateMediaTypeConfigurationResponseModel();

        return Task.FromResult<IActionResult>(Ok(responseModel));
    }
}
