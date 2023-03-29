using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Users;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.New.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Controllers.Users;

public class FilterUsersController : UsersControllerBase
{
    private readonly IUserService _userService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IUserPresentationFactory _userPresentationFactory;

    public FilterUsersController(
        IUserService userService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IUserPresentationFactory userPresentationFactory)
    {
        _userService = userService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _userPresentationFactory = userPresentationFactory;
    }

    /// <summary>
    /// Query users
    /// </summary>
    /// <param name="skip">Amount to skip.</param>
    /// <param name="take">Amount to take.</param>
    /// <param name="orderBy">Property to order by.</param>
    /// <param name="orderDirection">Direction to order in.</param>
    /// <param name="userGroupIds">Keys of the user groups to include in the result.</param>
    /// <param name="userStates">User states to include in the result.</param>
    /// <param name="filter">A string that must be present in the users name or username.</param>
    /// <returns>A paged result of the users matching the query.</returns>
    [HttpGet("filter")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Filter(
        int skip = 0,
        int take = 100,
        UserOrder orderBy = UserOrder.UserName,
        Direction orderDirection = Direction.Ascending,
        [FromQuery] SortedSet<Guid>? userGroupIds = null,
        [FromQuery] SortedSet<UserState>? userStates = null,
        string filter = "")
    {
        var userFilter = new UserFilter
        {
            IncludedUserGroups = userGroupIds,
            IncludeUserStates = userStates,
            NameFilters = string.IsNullOrEmpty(filter) ? null : new SortedSet<string> { filter }
        };

        // FIXME: use the actual currently logged in user key
        Attempt<PagedModel<IUser>, UserOperationStatus> filterAttempt =
            await _userService.FilterAsync(Constants.Security.SuperUserKey, userFilter, skip, take, orderBy, orderDirection);

        if (filterAttempt.Success is false)
        {
            return UserOperationStatusResult(filterAttempt.Status);
        }

        var responseModel = new PagedViewModel<UserResponseModel>
        {
            Total = filterAttempt.Result.Total,
            Items = filterAttempt.Result.Items.Select(_userPresentationFactory.CreateResponseModel).ToArray()
        };

        return Ok(responseModel);
    }
}
