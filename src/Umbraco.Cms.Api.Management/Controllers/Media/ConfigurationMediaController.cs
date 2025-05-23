using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Media;

namespace Umbraco.Cms.Api.Management.Controllers.Media;

[ApiVersion("1.0")]
[Obsolete("No longer used. Scheduled for removal in Umbraco 18.")]
public class ConfigurationMediaController : MediaControllerBase
{
    private readonly IConfigurationPresentationFactory _configurationPresentationFactory;


    public ConfigurationMediaController(IConfigurationPresentationFactory configurationPresentationFactory)
        => _configurationPresentationFactory = configurationPresentationFactory;

    [HttpGet("configuration")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(MediaConfigurationResponseModel), StatusCodes.Status200OK)]
    public Task<IActionResult> Configuration(CancellationToken cancellationToken)
    {
        MediaConfigurationResponseModel responseModel = _configurationPresentationFactory.CreateMediaConfigurationResponseModel();
        return Task.FromResult<IActionResult>(Ok(responseModel));
    }
}
