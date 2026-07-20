using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.UserGroup;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.User.Filter;

/// <summary>
/// Provides API endpoints for filtering and retrieving user groups in the management interface.
/// </summary>
[ApiVersion("1.0")]
public class FilterUserGroupFilterController : UserGroupFilterControllerBase
{
    private readonly IUserGroupService _userGroupService;
    private readonly IUserGroupPresentationFactory _userGroupPresentationFactory;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="FilterUserGroupFilterController"/> class, responsible for handling user group filter operations in the management API.
    /// </summary>
    /// <param name="userGroupService">Service for managing user group data and operations.</param>
    /// <param name="userGroupPresentationFactory">Factory for creating user group presentation models.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security context and authentication.</param>
    public FilterUserGroupFilterController(
        IUserGroupService userGroupService,
        IUserGroupPresentationFactory userGroupPresentationFactory,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _userGroupService = userGroupService;
        _userGroupPresentationFactory = userGroupPresentationFactory;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    /// <summary>
    /// Retrieves a paginated and filtered list of user groups based on the specified criteria.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="skip">The number of user groups to skip before starting to collect the result set.</param>
    /// <param name="take">The maximum number of user groups to return.</param>
    /// <param name="filter">An optional filter string to search or filter user groups by name or other criteria.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains an <see cref="IActionResult"/> with a <see cref="PagedViewModel{UserGroupResponseModel}"/> representing the filtered user groups.</returns>
    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<UserGroupResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Gets a filtered collection of user groups.")]
    [EndpointDescription("Filters user groups based on the provided criteria with support for pagination.")]
    public async Task<IActionResult> Filter(
        CancellationToken cancellationToken,
        int skip = 0,
        int take = 100,
        string filter = "")
    {
        Attempt<PagedModel<IUserGroup>, UserGroupOperationStatus> filterAttempt = await _userGroupService.FilterAsync(
            CurrentUserKey(_backOfficeSecurityAccessor),
            filter,
            skip,
            take);

        if (filterAttempt.Success is false)
        {
            return UserGroupOperationStatusResult(filterAttempt.Status);
        }

        IEnumerable<UserGroupResponseModel> viewModels = await _userGroupPresentationFactory.CreateMultipleAsync(filterAttempt.Result.Items);
        var responseModel = new PagedViewModel<UserGroupResponseModel>
        {
            Total = filterAttempt.Result.Total,
            Items = viewModels,
        };

        return Ok(responseModel);
    }
}
