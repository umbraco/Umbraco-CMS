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
