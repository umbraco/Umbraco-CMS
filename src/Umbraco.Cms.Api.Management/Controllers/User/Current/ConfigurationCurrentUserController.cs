using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.User.Current;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.User.Current;

/// <summary>
/// Provides API endpoints for managing configuration settings specific to the current user.
/// </summary>
[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.BackOfficeAccess)]
public class ConfigurationCurrentUserController : CurrentUserControllerBase
{
    private readonly IUserPresentationFactory _userPresentationFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationCurrentUserController"/> class.
    /// </summary>
    /// <param name="userPresentationFactory">Factory responsible for creating user presentation models.</param>
    public ConfigurationCurrentUserController(IUserPresentationFactory userPresentationFactory) => _userPresentationFactory = userPresentationFactory;

    /// <summary>
    /// Retrieves the configuration settings for the current user.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> containing a <see cref="CurrentUserConfigurationResponseModel"/> with the current user's configuration settings, returned with HTTP 200 (OK).
    /// </returns>
    [MapToApiVersion("1.0")]
    [HttpGet("configuration")]
    [ProducesResponseType(typeof(CurrentUserConfigurationResponseModel), StatusCodes.Status200OK)]
    [EndpointSummary("Gets the current user's configuration.")]
    [EndpointDescription("Gets the configuration settings for the current user.")]
    public async Task<IActionResult> Configuration(CancellationToken cancellationToken)
    {
        CurrentUserConfigurationResponseModel model = await _userPresentationFactory.CreateCurrentUserConfigurationModelAsync();
        return Ok(model);
    }
}
