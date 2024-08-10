using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Features;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentType;

[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessDocumentTypes)]
public class ConfigurationDocumentTypeController : DocumentTypeControllerBase
{
    private readonly IConfigurationPresentationFactory _configurationPresentationFactory;

    [ActivatorUtilitiesConstructor]
    public ConfigurationDocumentTypeController(IConfigurationPresentationFactory configurationPresentationFactory)
    {
        _configurationPresentationFactory = configurationPresentationFactory;
    }

    [Obsolete("Use the constructor that only accepts IConfigurationPresentationFactory, scheduled for removal in V16")]
    public ConfigurationDocumentTypeController(
        UmbracoFeatures umbracoFeatures,
        IOptionsSnapshot<DataTypesSettings> dataTypesSettings,
        IOptionsSnapshot<SegmentSettings> segmentSettings,
        IConfigurationPresentationFactory configurationPresentationFactory)
    : this(configurationPresentationFactory)
    {
    }

    [Obsolete("Use the constructor that only accepts IConfigurationPresentationFactory, scheduled for removal in V16")]
    public ConfigurationDocumentTypeController(
        UmbracoFeatures umbracoFeatures,
        IOptionsSnapshot<DataTypesSettings> dataTypesSettings,
        IOptionsSnapshot<SegmentSettings> segmentSettings)
    : this(StaticServiceProvider.Instance.GetRequiredService<IConfigurationPresentationFactory>())
    {
    }

    [HttpGet("configuration")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(DocumentTypeConfigurationResponseModel), StatusCodes.Status200OK)]
    public Task<IActionResult> Configuration(CancellationToken cancellationToken)
    {
        DocumentTypeConfigurationResponseModel responseModel = _configurationPresentationFactory.CreateDocumentTypeConfigurationResponseModel();

        return Task.FromResult<IActionResult>(Ok(responseModel));
    }
}
