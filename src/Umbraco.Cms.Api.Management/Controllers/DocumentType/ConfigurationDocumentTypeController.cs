using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentType;

/// <summary>
/// Controller responsible for managing the configuration settings of document types in the CMS.
/// </summary>
[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessDocumentTypes)]
public class ConfigurationDocumentTypeController : DocumentTypeControllerBase
{
    private readonly IConfigurationPresentationFactory _configurationPresentationFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationDocumentTypeController"/> class, which manages configuration-related operations for document types.
    /// </summary>
    /// <param name="configurationPresentationFactory">The factory used to create configuration presentation models for document types.</param>
    public ConfigurationDocumentTypeController(IConfigurationPresentationFactory configurationPresentationFactory)
        => _configurationPresentationFactory = configurationPresentationFactory;

    /// <summary>
    /// Retrieves the configuration settings for document types.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>An <see cref="IActionResult"/> containing a <see cref="DocumentTypeConfigurationResponseModel"/> with the document type configuration settings.</returns>
    [HttpGet("configuration")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(DocumentTypeConfigurationResponseModel), StatusCodes.Status200OK)]
    [EndpointSummary("Gets the document type configuration.")]
    [EndpointDescription("Gets the configuration settings for document types.")]
    public Task<IActionResult> Configuration(CancellationToken cancellationToken)
    {
        DocumentTypeConfigurationResponseModel responseModel = _configurationPresentationFactory.CreateDocumentTypeConfigurationResponseModel();

        return Task.FromResult<IActionResult>(Ok(responseModel));
    }
}
