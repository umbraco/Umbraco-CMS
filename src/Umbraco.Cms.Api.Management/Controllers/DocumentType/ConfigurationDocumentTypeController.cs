using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentType;

[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessDocumentTypes)]
public class ConfigurationDocumentTypeController : DocumentTypeControllerBase
{
    private readonly IConfigurationPresentationFactory _configurationPresentationFactory;

    public ConfigurationDocumentTypeController(IConfigurationPresentationFactory configurationPresentationFactory)
        => _configurationPresentationFactory = configurationPresentationFactory;

    [HttpGet("configuration")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(DocumentTypeConfigurationResponseModel), StatusCodes.Status200OK)]
    public Task<IActionResult> Configuration(CancellationToken cancellationToken)
    {
        DocumentTypeConfigurationResponseModel responseModel = _configurationPresentationFactory.CreateDocumentTypeConfigurationResponseModel();

        return Task.FromResult<IActionResult>(Ok(responseModel));
    }
}
