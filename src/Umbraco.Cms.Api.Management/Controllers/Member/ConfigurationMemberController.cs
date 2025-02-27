using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Member;

namespace Umbraco.Cms.Api.Management.Controllers.Member;

[ApiVersion("1.0")]
public class ConfigurationMemberController : MemberControllerBase
{
    private readonly IConfigurationPresentationFactory _configurationPresentationFactory;

    public ConfigurationMemberController(
        IConfigurationPresentationFactory configurationPresentationFactory) =>
        _configurationPresentationFactory = configurationPresentationFactory;

    [HttpGet("configuration")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(MemberConfigurationResponseModel), StatusCodes.Status200OK)]
    public Task<IActionResult> Configuration(CancellationToken cancellationToken)
    {
        MemberConfigurationResponseModel responseModel = _configurationPresentationFactory.CreateMemberConfigurationResponseModel();
        return Task.FromResult<IActionResult>(Ok(responseModel));
    }
}
