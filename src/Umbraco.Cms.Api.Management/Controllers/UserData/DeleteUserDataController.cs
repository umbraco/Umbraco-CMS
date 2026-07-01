using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.UserData;

/// <summary>
/// Controller responsible for managing API endpoints related to the deletion of user data.
/// </summary>
[ApiVersion("1.0")]
public class DeleteUserDataController : UserDataControllerBase
{
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IUserDataService _userDataService;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Controllers.UserData.DeleteUserDataController"/> class.
    /// </summary>
    /// <param name="backOfficeSecurityAccessor">Provides access to back office security features for authentication and authorization.</param>
    /// <param name="userDataService">Service used to manage user data operations.</param>
    public DeleteUserDataController(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IUserDataService userDataService)
    {
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _userDataService = userDataService;
    }

    /// <summary>
    /// Deletes the user data associated with the specified unique identifier.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier of the user data to delete.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the outcome of the operation: <c>200 OK</c> if successful, <c>400 Bad Request</c> or <c>404 Not Found</c> if the operation fails.</returns>
    [HttpDelete("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(UserDataOperationStatus), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(UserDataOperationStatus), StatusCodes.Status404NotFound)]
    [EndpointSummary("Deletes user data.")]
    [EndpointDescription("Deletes user data identified by the provided Id.")]
    public async Task<IActionResult> Delete(CancellationToken cancellationToken, Guid id)
    {
        IUserData? data = await _userDataService.GetAsync(id);
        if (data is null)
        {
            return NotFound();
        }

        Guid currentUserKey = CurrentUserKey(_backOfficeSecurityAccessor);

        if (data.UserKey != currentUserKey)
        {
            return Unauthorized();
        }

        Attempt<UserDataOperationStatus> attempt = await _userDataService.DeleteAsync(id);

        return attempt.Success
            ? Ok()
            : UserDataOperationStatusResult(attempt.Result);
    }
}
