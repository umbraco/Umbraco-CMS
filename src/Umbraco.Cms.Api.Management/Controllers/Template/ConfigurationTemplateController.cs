using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Template;
using Umbraco.Cms.Core.Features;

namespace Umbraco.Cms.Api.Management.Controllers.Template;

/// <summary>
/// Provides API endpoints for managing configuration templates within the Umbraco CMS.
/// </summary>
[ApiVersion("1.0")]
public class ConfigurationTemplateController : TemplateControllerBase
{
    private readonly UmbracoFeatures _umbracoFeatures;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationTemplateController"/> class with the specified Umbraco features.
    /// </summary>
    /// <param name="umbracoFeatures">An instance of <see cref="UmbracoFeatures"/> providing feature configuration for the controller.</param>
    public ConfigurationTemplateController(UmbracoFeatures umbracoFeatures) => _umbracoFeatures = umbracoFeatures;

    [HttpGet("configuration")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(TemplateConfigurationResponseModel), StatusCodes.Status200OK)]
    [EndpointSummary("Gets the template configuration.")]
    [EndpointDescription("Gets the configuration settings for templates.")]
    public Task<IActionResult> Configuration(CancellationToken cancellationToken)
    {
        var responseModel = new TemplateConfigurationResponseModel
        {
            Disabled = _umbracoFeatures.Disabled.DisableTemplates,
        };

        return Task.FromResult<IActionResult>(Ok(responseModel));
    }
}
