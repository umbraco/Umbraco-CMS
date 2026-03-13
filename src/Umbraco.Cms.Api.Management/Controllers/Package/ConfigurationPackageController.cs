using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Package;

namespace Umbraco.Cms.Api.Management.Controllers.Package;

/// <summary>
/// Provides API endpoints for managing configuration packages within Umbraco CMS.
/// </summary>
[ApiVersion("1.0")]
public class ConfigurationPackageController : PackageControllerBase
{
    private readonly IPackagePresentationFactory _packagePresentationFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationPackageController"/>.
    /// </summary>
    /// <param name="packagePresentationFactory">Factory for creating package presentations.</param>
    public ConfigurationPackageController(IPackagePresentationFactory packagePresentationFactory) => _packagePresentationFactory = packagePresentationFactory;

    [HttpGet("configuration")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PackageConfigurationResponseModel), StatusCodes.Status200OK)]
    [EndpointSummary("Gets the package configuration.")]
    [EndpointDescription("Gets the configuration settings for packages.")]
    public Task<IActionResult> Configuration(CancellationToken cancellationToken)
    {
        PackageConfigurationResponseModel responseModel = _packagePresentationFactory.CreateConfigurationResponseModel();
        return Task.FromResult<IActionResult>(Ok(responseModel));
    }
}
