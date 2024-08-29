using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Template;
using Umbraco.Cms.Core.Features;

namespace Umbraco.Cms.Api.Management.Controllers.Template;

[ApiVersion("1.0")]
public class ConfigurationTemplateController : TemplateControllerBase
{
    private readonly UmbracoFeatures _umbracoFeatures;

    public ConfigurationTemplateController(UmbracoFeatures umbracoFeatures) => _umbracoFeatures = umbracoFeatures;

    [HttpGet("configuration")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(TemplateConfigurationResponseModel), StatusCodes.Status200OK)]
    public Task<IActionResult> Configuration(CancellationToken cancellationToken)
    {
        var responseModel = new TemplateConfigurationResponseModel
        {
            Disabled = _umbracoFeatures.Disabled.DisableTemplates,
        };

        return Task.FromResult<IActionResult>(Ok(responseModel));
    }
}
