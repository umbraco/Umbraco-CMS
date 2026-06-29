using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.UserData;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Querying;

namespace Umbraco.Cms.Api.Management.Controllers.UserData;

/// <summary>
/// API controller responsible for managing user-specific data identified by a unique key.
/// Provides endpoints for retrieving, updating, and deleting user data entries by key.
/// </summary>
[ApiVersion("1.0")]
public class ByKeyUserDataController : UserDataControllerBase
{
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IUserDataService _userDataService;
    private readonly IUmbracoMapper _umbracoMapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="ByKeyUserDataController"/> class.
    /// </summary>
    /// <param name="backOfficeSecurityAccessor">Provides access to back office security information for the current request.</param>
    /// <param name="userDataService">Service used to manage and retrieve user-specific data.</param>
    /// <param name="umbracoMapper">Maps between Umbraco domain models and API models.</param>
    public ByKeyUserDataController(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IUserDataService userDataService,
        IUmbracoMapper umbracoMapper)
    {
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _userDataService = userDataService;
        _umbracoMapper = umbracoMapper;
    }

    /// <summary>
    /// Retrieves user data for the specified unique identifier (ID).
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier of the user data to retrieve.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> containing the user data if found and authorized;
    /// otherwise, returns <c>NotFound</c> if the data does not exist, or <c>Unauthorized</c> if the current user does not have access.
    /// </returns>
    [HttpGet("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(UserDataViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("Gets user data.")]
    [EndpointDescription("Gets user data identified by the provided Id.")]
    public async Task<IActionResult> ByKey(CancellationToken cancellationToken, Guid id)
    {
        Guid currentUserKey = CurrentUserKey(_backOfficeSecurityAccessor);
        IUserData? data = await _userDataService.GetAsync(id);
        if (data is null)
        {
            return NotFound();
        }

        if (data.UserKey != currentUserKey)
        {
            return Unauthorized();
        }

        return Ok(_umbracoMapper.Map<UserDataResponseModel>(data));
    }
}
