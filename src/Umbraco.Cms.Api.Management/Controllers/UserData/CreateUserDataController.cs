using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.UserData;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.UserData;

/// <summary>
/// API controller responsible for handling operations related to the creation of user data.
/// </summary>
[ApiVersion("1.0")]
public class CreateUserDataController : UserDataControllerBase
{
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IUserDataService _userDataService;
    private readonly IUmbracoMapper _umbracoMapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateUserDataController"/> class, responsible for handling user data creation operations in the management API.
    /// </summary>
    /// <param name="backOfficeSecurityAccessor">Provides access to back office security features for authentication and authorization.</param>
    /// <param name="userDataService">Service used to manage and persist user data.</param>
    /// <param name="umbracoMapper">The mapper used to convert between Umbraco domain models and API models.</param>
    public CreateUserDataController(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IUserDataService userDataService,
        IUmbracoMapper umbracoMapper)
    {
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _userDataService = userDataService;
        _umbracoMapper = umbracoMapper;
    }

    /// <summary>
    /// Creates user-specific data for the currently authenticated user using the provided key and value.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the operation.</param>
    /// <param name="model">A <see cref="CreateUserDataRequestModel"/> containing the key and value to associate with the current user.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> that represents the result of the create operation.
    /// Returns <c>201 Created</c> with the created resource location on success, <c>400 Bad Request</c> if the request is invalid, or <c>404 Not Found</c> if the user cannot be found.
    /// </returns>
    [HttpPost]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(UserDataOperationStatus), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(UserDataOperationStatus), StatusCodes.Status404NotFound)]
    [EndpointSummary("Creates user data.")]
    [EndpointDescription("Creates user-specific data for the current authenticated user with the provided key and value.")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken, CreateUserDataRequestModel model)
    {
        Guid currentUserKey = CurrentUserKey(_backOfficeSecurityAccessor);

        IUserData userData = _umbracoMapper.Map<IUserData>(model)!;
        userData.UserKey = currentUserKey;

        Attempt<IUserData, UserDataOperationStatus> attempt = await _userDataService.CreateAsync(userData);


        return attempt.Success
            ? CreatedAtId<ByKeyUserDataController>(controller => nameof(controller.ByKey), attempt.Result.Key)
            : UserDataOperationStatusResult(attempt.Status);
    }
}
