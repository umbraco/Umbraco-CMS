using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.User;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.User;

[ApiVersion("1.0")]
public class UpdateUserController : UserControllerBase
{
    private readonly IUserService _userService;
    private readonly IUserPresentationFactory _userPresentationFactory;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public UpdateUserController(
        IUserService userService,
        IUserPresentationFactory userPresentationFactory, IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _userService = userService;
        _userPresentationFactory = userPresentationFactory;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpPut("{id:guid}")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Update(Guid id, UpdateUserRequestModel model)
    {
        // We have to use an intermediate save model, and cannot map it directly to an IUserModel
        // This is because we need to compare the updated values with what the user already has, for audit purposes.
        UserUpdateModel updateModel = await _userPresentationFactory.CreateUpdateModelAsync(id, model);

        Attempt<IUser?, UserOperationStatus> result = await _userService.UpdateAsync(CurrentUserKey(_backOfficeSecurityAccessor), updateModel);

        return result.Success
            ? Ok()
            : UserOperationStatusResult(result.Status);
    }
}
