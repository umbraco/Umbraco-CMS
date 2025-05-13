using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.User;

namespace Umbraco.Cms.Api.Management.Controllers.User;

[ApiVersion("1.0")]
public class ConfigurationUserController : UserControllerBase
{
    private readonly IUserPresentationFactory _userPresentationFactory;

    public ConfigurationUserController(IUserPresentationFactory userPresentationFactory) => _userPresentationFactory = userPresentationFactory;

    [HttpGet("configuration")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(UserConfigurationResponseModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> Configuration(CancellationToken cancellationToken)
        => Ok(await _userPresentationFactory.CreateUserConfigurationModelAsync());
}
