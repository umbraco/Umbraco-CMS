using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Security;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Security;

[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.DenyLocalLoginIfConfigured)]
public class ConfigurationSecurityController : SecurityControllerBase
{
    private readonly IPasswordConfigurationPresentationFactory _passwordConfigurationPresentationFactory;

    public ConfigurationSecurityController(IPasswordConfigurationPresentationFactory passwordConfigurationPresentationFactory)
        => _passwordConfigurationPresentationFactory = passwordConfigurationPresentationFactory;

    [HttpGet("configuration")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(SecurityConfigurationResponseModel), StatusCodes.Status200OK)]
    public Task<IActionResult> Configuration(CancellationToken cancellationToken)
    {
        var viewModel = new SecurityConfigurationResponseModel
        {
            PasswordConfiguration = _passwordConfigurationPresentationFactory.CreatePasswordConfigurationResponseModel(),
        };

        return Task.FromResult<IActionResult>(Ok(viewModel));
    }
}
