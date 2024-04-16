using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.User.Current;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.User.Current;

[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.BackOfficeAccess)]
public class ConfigurationCurrentUserController : CurrentUserControllerBase
{
    private readonly IUserPresentationFactory _userPresentationFactory;

    public ConfigurationCurrentUserController(IUserPresentationFactory userPresentationFactory) => _userPresentationFactory = userPresentationFactory;

    [MapToApiVersion("1.0")]
    [HttpGet("configuration")]
    [ProducesResponseType(typeof(CurrenUserConfigurationResponseModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> Configuration(CancellationToken cancellationToken)
    {
        CurrenUserConfigurationResponseModel model = await _userPresentationFactory.CreateCurrentUserConfigurationModelAsync();
        return Ok(model);
    }
}
