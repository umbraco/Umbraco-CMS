using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.UserGroup;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.UserGroup;

[ApiVersion("1.0")]
public class ByKeyUserGroupController : UserGroupControllerBase
{
    private readonly IUserGroupService _userGroupService;
    private readonly IUserGroupPresentationFactory _userGroupPresentationFactory;
    private readonly IAuthorizationService _authorizationService;

    public ByKeyUserGroupController(
        IUserGroupService userGroupService,
        IUserGroupPresentationFactory userGroupPresentationFactory,
        IAuthorizationService authorizationService)
    {
        _userGroupService = userGroupService;
        _userGroupPresentationFactory = userGroupPresentationFactory;
        _authorizationService = authorizationService;
    }

    [HttpGet("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(UserGroupResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ByKey(Guid id)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeAsync(User, new[] { id },
            $"New{AuthorizationPolicies.UserBelongsToUserGroupInRequest}");

        if (!authorizationResult.Succeeded)
        {
            return new StatusCodeResult(StatusCodes.Status403Forbidden);
        }

        IUserGroup? userGroup = await _userGroupService.GetAsync(id);

        if (userGroup is null)
        {
            return UserGroupNotFound();
        }

        return Ok(await _userGroupPresentationFactory.CreateAsync(userGroup));
    }
}
