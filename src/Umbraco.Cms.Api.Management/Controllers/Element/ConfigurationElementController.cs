using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Element;

namespace Umbraco.Cms.Api.Management.Controllers.Element;

[ApiVersion("1.0")]
public class ConfigurationElementController : ElementControllerBase
{
    private readonly IConfigurationPresentationFactory _configurationPresentationFactory;

    public ConfigurationElementController(IConfigurationPresentationFactory configurationPresentationFactory)
        => _configurationPresentationFactory = configurationPresentationFactory;

    [HttpGet("configuration")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ElementConfigurationResponseModel), StatusCodes.Status200OK)]
    public Task<IActionResult> Configuration(CancellationToken cancellationToken)
    {
        ElementConfigurationResponseModel responseModel = _configurationPresentationFactory.CreateElementConfigurationResponseModel();
        return Task.FromResult<IActionResult>(Ok(responseModel));
    }
}
