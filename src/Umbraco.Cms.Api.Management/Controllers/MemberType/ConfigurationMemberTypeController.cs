using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.MemberType;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.MemberType;

[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessMemberTypes)]
public class ConfigurationMemberTypeController : MemberTypeControllerBase
{
    private readonly IConfigurationPresentationFactory _configurationPresentationFactory;

    public ConfigurationMemberTypeController(IConfigurationPresentationFactory configurationPresentationFactory)
    {
        _configurationPresentationFactory = configurationPresentationFactory;
    }

    [HttpGet("configuration")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(MemberTypeConfigurationResponseModel), StatusCodes.Status200OK)]
    public Task<IActionResult> Configuration(CancellationToken cancellationToken)
    {
        MemberTypeConfigurationResponseModel responseModel = _configurationPresentationFactory.CreateMemberTypeConfigurationResponseModel();

        return Task.FromResult<IActionResult>(Ok(responseModel));
    }
}
