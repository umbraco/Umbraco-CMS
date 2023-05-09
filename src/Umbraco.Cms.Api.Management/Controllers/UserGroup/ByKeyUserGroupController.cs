using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.UserGroup;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.UserGroup;

[ApiVersion("1.0")]
public class ByKeyUserGroupController : UserGroupControllerBase
{
    private readonly IUserGroupService _userGroupService;
    private readonly IUserGroupPresentationFactory _userGroupPresentationFactory;

    public ByKeyUserGroupController(
        IUserGroupService userGroupService,
        IUserGroupPresentationFactory userGroupPresentationFactory)
    {
        _userGroupService = userGroupService;
        _userGroupPresentationFactory = userGroupPresentationFactory;
    }

    [HttpGet("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(UserGroupResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserGroupResponseModel>> ByKey(Guid id)
    {
        IUserGroup? userGroup = await _userGroupService.GetAsync(id);

        if (userGroup is null)
        {
            return NotFound();
        }

        return await _userGroupPresentationFactory.CreateAsync(userGroup);
    }
}
