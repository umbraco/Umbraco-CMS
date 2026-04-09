using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.User;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Exceptions;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Controllers.User;

/// <summary>
/// API controller responsible for retrieving information about all users in the system.
/// </summary>
[ApiVersion("1.0")]
public class GetAllUserController : UserControllerBase
{
    private readonly IUserService _userService;
    private readonly IUserPresentationFactory _userPresentationFactory;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Controllers.User.GetAllUserController"/> class, which handles requests to retrieve all users.
    /// </summary>
    /// <param name="userService">Service used for user management operations.</param>
    /// <param name="userPresentationFactory">Factory for creating user presentation models.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security context.</param>
    public GetAllUserController(
        IUserService userService,
        IUserPresentationFactory userPresentationFactory,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _userService = userService;
        _userPresentationFactory = userPresentationFactory;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    /// <summary>
    /// Retrieves a paginated collection of all users.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="skip">The number of users to skip before starting to collect the result set. Used for pagination.</param>
    /// <param name="take">The maximum number of users to return. Used for pagination.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains an <see cref="IActionResult"/> with a <see cref="PagedViewModel{UserResponseModel}"/> representing the paginated collection of users.
    /// Returns 200 OK with the collection, or 404 Not Found if users cannot be retrieved.
    /// </returns>
    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<UserResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Gets a paginated collection of users.")]
    [EndpointDescription("Gets a paginated collection of all users.")]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken, int skip = 0, int take = 100)
    {
        Attempt<PagedModel<IUser>?, UserOperationStatus> attempt = await _userService.GetAllAsync(CurrentUserKey(_backOfficeSecurityAccessor), skip, take);

        if (attempt.Success is false)
        {
            return UserOperationStatusResult(attempt.Status);
        }

        PagedModel<IUser>? result = attempt.Result;
        if (result is null)
        {
            throw new PanicException("Get all attempt succeeded, but result was null");
        }

        var pagedViewModel = new PagedViewModel<UserResponseModel>
        {
            Total = result.Total,
            Items = result.Items.Select(x => _userPresentationFactory.CreateResponseModel(x))
        };

        return Ok(pagedViewModel);
    }
}
