using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.UserGroup;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.UserGroup;

[ApiVersion("1.0")]
public class UpdateUserGroupController : UserGroupControllerBase
{
    private readonly IUserGroupService _userGroupService;
    private readonly IUserGroupPresentationFactory _userGroupPresentationFactory;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public UpdateUserGroupController(
        IUserGroupService userGroupService,
        IUserGroupPresentationFactory userGroupPresentationFactory,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _userGroupService = userGroupService;
        _userGroupPresentationFactory = userGroupPresentationFactory;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpPut("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        CancellationToken cancellationToken,
        Guid id,
        UpdateUserGroupRequestModel updateUserGroupRequestModel)
    {
        IUserGroup? existingUserGroup = await _userGroupService.GetAsync(id);

        if (existingUserGroup is null)
        {
            return UserGroupOperationStatusResult(UserGroupOperationStatus.NotFound);
        }

        Attempt<IUserGroup, UserGroupOperationStatus> userGroupUpdateAttempt = await _userGroupPresentationFactory.UpdateAsync(existingUserGroup, updateUserGroupRequestModel);
        if (userGroupUpdateAttempt.Success is false)
        {
            return UserGroupOperationStatusResult(userGroupUpdateAttempt.Status);
        }

        IUserGroup userGroup = userGroupUpdateAttempt.Result;
        Attempt<IUserGroup, UserGroupOperationStatus> result = await _userGroupService.UpdateAsync(userGroup, CurrentUserKey(_backOfficeSecurityAccessor));

        return result.Success
            ? Ok()
            : UserGroupOperationStatusResult(result.Status);
    }
}
