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
public class CreateUserGroupController : UserGroupControllerBase
{
    private readonly IUserGroupService _userGroupService;
    private readonly IUserGroupPresentationFactory _userGroupPresentationFactory;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public CreateUserGroupController(
        IUserGroupService userGroupService,
        IUserGroupPresentationFactory userGroupPresentationFactory,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _userGroupService = userGroupService;
        _userGroupPresentationFactory = userGroupPresentationFactory;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpPost]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(CancellationToken cancellationToken, CreateUserGroupRequestModel createUserGroupRequestModel)
    {
        Attempt<IUserGroup, UserGroupOperationStatus> userGroupCreationAttempt = await _userGroupPresentationFactory.CreateAsync(createUserGroupRequestModel);
        if (userGroupCreationAttempt.Success is false)
        {
            return UserGroupOperationStatusResult(userGroupCreationAttempt.Status);
        }

        IUserGroup group = userGroupCreationAttempt.Result;

        Attempt<IUserGroup, UserGroupOperationStatus> result = await _userGroupService.CreateAsync(group, CurrentUserKey(_backOfficeSecurityAccessor));
        return result.Success
            ? CreatedAtId<ByKeyUserGroupController>(controller => nameof(controller.ByKey), group.Key)
            : UserGroupOperationStatusResult(result.Status);
    }
}
