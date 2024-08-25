using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.UserGroup;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.UserGroup;

[ApiVersion("1.0")]
public class ByKeyUserGroupController : UserGroupControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IUserGroupService _userGroupService;
    private readonly IUserGroupPresentationFactory _userGroupPresentationFactory;

    public ByKeyUserGroupController(
        IAuthorizationService authorizationService,
        IUserGroupService userGroupService,
        IUserGroupPresentationFactory userGroupPresentationFactory)
    {
        _authorizationService = authorizationService;
        _userGroupService = userGroupService;
        _userGroupPresentationFactory = userGroupPresentationFactory;
    }

    [HttpGet("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(UserGroupResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ByKey(CancellationToken cancellationToken, Guid id)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            UserGroupPermissionResource.WithKeys(id),
            AuthorizationPolicies.UserBelongsToUserGroupInRequest);

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        IUserGroup? userGroup = await _userGroupService.GetAsync(id);

        if (userGroup is null)
        {
            return UserGroupNotFound();
        }

        return Ok(await _userGroupPresentationFactory.CreateAsync(userGroup));
    }
}
