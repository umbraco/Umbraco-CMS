using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Element;

namespace Umbraco.Cms.Api.Management.Controllers.Element;

/// <summary>
/// API controller responsible for handling operations related to configuration elements within the Umbraco CMS management area.
/// </summary>
[ApiVersion("1.0")]
public class ConfigurationElementController : ElementControllerBase
{
    private readonly IConfigurationPresentationFactory _configurationPresentationFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationElementController"/> class.
    /// </summary>
    /// <param name="configurationPresentationFactory">Factory used to create configuration presentation models for elements.</param>
    public ConfigurationElementController(IConfigurationPresentationFactory configurationPresentationFactory)
        => _configurationPresentationFactory = configurationPresentationFactory;

    /// <summary>
    /// Retrieves the configuration settings for elements.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the operation.</param>
    /// <returns>An <see cref="IActionResult"/> containing an <see cref="ElementConfigurationResponseModel"/> with the element configuration settings.</returns>
    [HttpGet("configuration")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ElementConfigurationResponseModel), StatusCodes.Status200OK)]
    [EndpointSummary("Gets the element configuration.")]
    [EndpointDescription("Gets the configuration settings for elements.")]
    public Task<IActionResult> Configuration(CancellationToken cancellationToken)
    {
        ElementConfigurationResponseModel responseModel = _configurationPresentationFactory.CreateElementConfigurationResponseModel();
        return Task.FromResult<IActionResult>(Ok(responseModel));
    }
}
