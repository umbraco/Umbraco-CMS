using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Package;

namespace Umbraco.Cms.Api.Management.Controllers.Package;

[ApiVersion("1.0")]
public class ConfigurationPackageController : PackageControllerBase
{
    private readonly IPackagePresentationFactory _packagePresentationFactory;

    public ConfigurationPackageController(IPackagePresentationFactory packagePresentationFactory) => _packagePresentationFactory = packagePresentationFactory;

    [HttpGet("configuration")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PackageConfigurationResponseModel), StatusCodes.Status200OK)]
    public Task<IActionResult> Configuration(CancellationToken cancellationToken)
    {
        PackageConfigurationResponseModel responseModel = _packagePresentationFactory.CreateConfigurationResponseModel();
        return Task.FromResult<IActionResult>(Ok(responseModel));
    }
}
