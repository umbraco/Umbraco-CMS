using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Security;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Security;

/// <summary>
/// Provides API endpoints for managing security-related configuration settings in the application.
/// </summary>
[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.DenyLocalLoginIfConfigured)]
public class ConfigurationSecurityController : SecurityControllerBase
{
    private readonly IPasswordConfigurationPresentationFactory _passwordConfigurationPresentationFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationSecurityController"/> class.
    /// </summary>
    /// <param name="passwordConfigurationPresentationFactory">Factory used to create password configuration presentation models.</param>
    public ConfigurationSecurityController(IPasswordConfigurationPresentationFactory passwordConfigurationPresentationFactory)
        => _passwordConfigurationPresentationFactory = passwordConfigurationPresentationFactory;

    [HttpGet("configuration")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(SecurityConfigurationResponseModel), StatusCodes.Status200OK)]
    [EndpointSummary("Gets the security configuration.")]
    [EndpointDescription("Gets the configuration settings for security.")]
    public Task<IActionResult> Configuration(CancellationToken cancellationToken)
    {
        var viewModel = new SecurityConfigurationResponseModel
        {
            PasswordConfiguration = _passwordConfigurationPresentationFactory.CreatePasswordConfigurationResponseModel(),
        };

        return Task.FromResult<IActionResult>(Ok(viewModel));
    }
}
