using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Security.Authorization.UserGroup;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.UserGroup;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.UserGroup;

[ApiVersion("1.0")]
public class AddUsersToUserGroupController : UserGroupControllerBase
{
    private readonly IUserGroupService _userGroupService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IAuthorizationService _authorizationService;

    public AddUsersToUserGroupController(
        IUserGroupService userGroupService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IAuthorizationService authorizationService)
    {
        _userGroupService = userGroupService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _authorizationService = authorizationService;
    }

    [HttpPost("{id:guid}/users")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(CancellationToken cancellationToken, Guid id, ReferenceByIdModel[] userIds)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            UserGroupPermissionResource.WithKeys(id),
            AuthorizationPolicies.UserBelongsToUserGroupInRequest);

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        Attempt<UserGroupOperationStatus> result = await _userGroupService.AddUsersToUserGroupAsync(
            new UsersToUserGroupManipulationModel(id, userIds.Select(x => x.Id).ToArray()), CurrentUserKey(_backOfficeSecurityAccessor));

        return result.Success
            ? Ok()
            : UserGroupOperationStatusResult(result.Result);
    }
}
