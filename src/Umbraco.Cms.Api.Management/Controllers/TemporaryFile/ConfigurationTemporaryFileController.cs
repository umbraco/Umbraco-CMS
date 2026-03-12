using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.TemporaryFile;

namespace Umbraco.Cms.Api.Management.Controllers.TemporaryFile;

/// <summary>
/// Provides API endpoints for managing temporary files related to configuration in the system.
/// </summary>
[ApiVersion("1.0")]
public class ConfigurationTemporaryFileController : TemporaryFileControllerBase
{
    private readonly ITemporaryFileConfigurationPresentationFactory _temporaryFileConfigurationPresentationFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationTemporaryFileController"/> class.
    /// </summary>
    /// <param name="temporaryFileConfigurationPresentationFactory">
    /// The factory used to create presentation models for temporary file configurations.
    /// </param>
    public ConfigurationTemporaryFileController(
        ITemporaryFileConfigurationPresentationFactory temporaryFileConfigurationPresentationFactory) =>
        _temporaryFileConfigurationPresentationFactory = temporaryFileConfigurationPresentationFactory;

    /// <summary>
    /// Retrieves the configuration settings for temporary files.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains an <see cref="IActionResult"/> with a <see cref="TemporaryFileConfigurationResponseModel"/> representing the temporary file configuration.</returns>
    [HttpGet("configuration")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(TemporaryFileConfigurationResponseModel), StatusCodes.Status200OK)]
    [EndpointSummary("Gets the temporary file configuration.")]
    [EndpointDescription("Gets the configuration settings for temporary files.")]
    public Task<IActionResult> Configuration(CancellationToken cancellationToken)
    {
        TemporaryFileConfigurationResponseModel responseModel = _temporaryFileConfigurationPresentationFactory.Create();
        return Task.FromResult<IActionResult>(Ok(responseModel));
    }
}
