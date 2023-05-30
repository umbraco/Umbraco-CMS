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

[ApiVersion("1.0")]
public class GetAllUserGroupController : UserGroupControllerBase
{
    private readonly IUserGroupService _userGroupService;
    private readonly IUserGroupPresentationFactory _userPresentationFactory;

    public GetAllUserGroupController(
        IUserGroupService userGroupService,
        IUserGroupPresentationFactory userPresentationFactory)
    {
        _userGroupService = userGroupService;
        _userPresentationFactory = userPresentationFactory;
    }

    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<UserGroupResponseModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedViewModel<UserGroupResponseModel>>> GetAll(int skip = 0, int take = 100)
    {
        // FIXME: In the old controller this endpoint had a switch "onlyCurrentUserGroup"
        // If this was enabled we'd only return the groups the current user was in
        // and even if it was set to false we'd still remove the admin group.
        // We still need to have this functionality, however, it does not belong here.
        // Instead we should implement this functionality on the CurrentUserController
        PagedModel<IUserGroup> userGroups = await _userGroupService.GetAllAsync(skip, take);

        var viewModels = (await _userPresentationFactory.CreateMultipleAsync(userGroups.Items)).ToList();
        return new PagedViewModel<UserGroupResponseModel> { Total = userGroups.Total, Items = viewModels };
    }
}
