using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.MemberType;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.MemberType;

    /// <summary>
    /// Provides API endpoints for managing the configuration of member types in the Umbraco CMS.
    /// </summary>
[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessMemberTypes)]
public class ConfigurationMemberTypeController : MemberTypeControllerBase
{
    private readonly IConfigurationPresentationFactory _configurationPresentationFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationMemberTypeController"/> class.
    /// </summary>
    /// <param name="configurationPresentationFactory">Factory used to create configuration presentation models for member types.</param>
    public ConfigurationMemberTypeController(IConfigurationPresentationFactory configurationPresentationFactory)
    {
        _configurationPresentationFactory = configurationPresentationFactory;
    }

    /// <summary>
    /// Gets the configuration settings for member types.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>An <see cref="IActionResult"/> containing a <see cref="MemberTypeConfigurationResponseModel"/> with the member type configuration settings.</returns>
    [HttpGet("configuration")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(MemberTypeConfigurationResponseModel), StatusCodes.Status200OK)]
    [EndpointSummary("Gets the member type configuration.")]
    [EndpointDescription("Gets the configuration settings for member types.")]
    public Task<IActionResult> Configuration(CancellationToken cancellationToken)
    {
        MemberTypeConfigurationResponseModel responseModel = _configurationPresentationFactory.CreateMemberTypeConfigurationResponseModel();

        return Task.FromResult<IActionResult>(Ok(responseModel));
    }
}
