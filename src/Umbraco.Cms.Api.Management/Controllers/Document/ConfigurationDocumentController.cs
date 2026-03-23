using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Document;

namespace Umbraco.Cms.Api.Management.Controllers.Document;

/// <summary>
/// API controller responsible for handling operations related to configuration documents within the Umbraco CMS management area.
/// </summary>
[ApiVersion("1.0")]
public class ConfigurationDocumentController : DocumentControllerBase
{
    private readonly IConfigurationPresentationFactory _configurationPresentationFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationDocumentController"/> class, responsible for managing configuration documents.
    /// </summary>
    /// <param name="configurationPresentationFactory">Factory used to create configuration presentation models for documents.</param>
    public ConfigurationDocumentController(
        IConfigurationPresentationFactory configurationPresentationFactory) =>
        _configurationPresentationFactory = configurationPresentationFactory;

    /// <summary>
    /// Retrieves the configuration settings for documents.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the operation.</param>
    /// <returns>An <see cref="IActionResult"/> containing a <see cref="DocumentConfigurationResponseModel"/> with the document configuration settings.</returns>
    [HttpGet("configuration")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(DocumentConfigurationResponseModel), StatusCodes.Status200OK)]
    [EndpointSummary("Gets the document configuration.")]
    [EndpointDescription("Gets the configuration settings for documents.")]
    public Task<IActionResult> Configuration(CancellationToken cancellationToken)
    {
        DocumentConfigurationResponseModel responseModel = _configurationPresentationFactory.CreateDocumentConfigurationResponseModel();
        return Task.FromResult<IActionResult>(Ok(responseModel));
    }
}
