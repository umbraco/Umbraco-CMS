using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Document;

namespace Umbraco.Cms.Api.Management.Controllers.Document;

[ApiVersion("1.0")]
public class ConfigurationDocumentController : DocumentControllerBase
{
    private readonly IConfigurationPresentationFactory _configurationPresentationFactory;

    public ConfigurationDocumentController(
        IConfigurationPresentationFactory configurationPresentationFactory) =>
        _configurationPresentationFactory = configurationPresentationFactory;

    [HttpGet("configuration")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(DocumentConfigurationResponseModel), StatusCodes.Status200OK)]
    public Task<IActionResult> Configuration(CancellationToken cancellationToken)
    {
        DocumentConfigurationResponseModel responseModel = _configurationPresentationFactory.CreateDocumentConfigurationResponseModel();
        return Task.FromResult<IActionResult>(Ok(responseModel));
    }
}
