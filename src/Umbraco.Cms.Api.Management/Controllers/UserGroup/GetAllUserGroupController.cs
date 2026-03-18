using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.UserGroup;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Controllers.UserGroup;

/// <summary>
/// Controller responsible for retrieving all user groups.
/// </summary>
[ApiVersion("1.0")]
public class GetAllUserGroupController : UserGroupControllerBase
{
    private readonly IUserGroupService _userGroupService;
    private readonly IUserGroupPresentationFactory _userPresentationFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAllUserGroupController"/> class.
    /// </summary>
    /// <param name="userGroupService">Service for managing user groups.</param>
    /// <param name="userPresentationFactory">Factory for creating user group presentation models.</param>
    public GetAllUserGroupController(
        IUserGroupService userGroupService,
        IUserGroupPresentationFactory userPresentationFactory)
    {
        _userGroupService = userGroupService;
        _userPresentationFactory = userPresentationFactory;
    }

    /// <summary>
    /// Retrieves a paginated list of all user groups.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="skip">The number of user groups to skip before starting to collect the result set. Used for pagination.</param>
    /// <param name="take">The maximum number of user groups to return. Used for pagination.</param>
    /// <returns>
    /// A <see cref="PagedViewModel{UserGroupResponseModel}"/> containing the total number of user groups and a collection of user group response models for the current page.
    /// </returns>
    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<UserGroupResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a paginated collection of user groups.")]
    [EndpointDescription("Gets a paginated collection of all user groups.")]
    public async Task<ActionResult<PagedViewModel<UserGroupResponseModel>>> GetAll(
        CancellationToken cancellationToken,
        int skip = 0,
        int take = 100)
    {
        PagedModel<IUserGroup> userGroups = await _userGroupService.GetAllAsync(skip, take);

        var viewModels = (await _userPresentationFactory.CreateMultipleAsync(userGroups.Items)).ToList();
        return new PagedViewModel<UserGroupResponseModel> { Total = userGroups.Total, Items = viewModels };
    }
}
