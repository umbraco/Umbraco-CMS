using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.TemporaryFile;

namespace Umbraco.Cms.Api.Management.Controllers.TemporaryFile;

[ApiVersion("1.0")]
public class ConfigurationTemporaryFileController : TemporaryFileControllerBase
{
    private readonly ITemporaryFileConfigurationPresentationFactory _temporaryFileConfigurationPresentationFactory;

    public ConfigurationTemporaryFileController(
        ITemporaryFileConfigurationPresentationFactory temporaryFileConfigurationPresentationFactory) =>
        _temporaryFileConfigurationPresentationFactory = temporaryFileConfigurationPresentationFactory;

    [HttpGet("configuration")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(TemporaryFileConfigurationResponseModel), StatusCodes.Status200OK)]
    public Task<IActionResult> Configuration(CancellationToken cancellationToken)
    {
        TemporaryFileConfigurationResponseModel responseModel = _temporaryFileConfigurationPresentationFactory.Create();
        return Task.FromResult<IActionResult>(Ok(responseModel));
    }
}
