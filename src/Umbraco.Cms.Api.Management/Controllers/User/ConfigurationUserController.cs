using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.User;

namespace Umbraco.Cms.Api.Management.Controllers.User;

/// <summary>
/// Provides API endpoints for managing configuration settings related to users.
/// </summary>
[ApiVersion("1.0")]
public class ConfigurationUserController : UserControllerBase
{
    private readonly IUserPresentationFactory _userPresentationFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Controllers.User.ConfigurationUserController"/> class, which manages user configuration operations in the Umbraco management API.
    /// </summary>
    /// <param name="userPresentationFactory">The factory used to create user presentation models.</param>
    public ConfigurationUserController(IUserPresentationFactory userPresentationFactory) => _userPresentationFactory = userPresentationFactory;

    /// <summary>
    /// Retrieves the configuration settings for the current user.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>An <see cref="IActionResult"/> containing a <see cref="UserConfigurationResponseModel"/> with the user's configuration settings.</returns>
    [HttpGet("configuration")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(UserConfigurationResponseModel), StatusCodes.Status200OK)]
    [EndpointSummary("Gets the user configuration.")]
    [EndpointDescription("Gets the configuration settings for users.")]
    public async Task<IActionResult> Configuration(CancellationToken cancellationToken)
        => Ok(await _userPresentationFactory.CreateUserConfigurationModelAsync());
}
