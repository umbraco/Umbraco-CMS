using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.User;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.User;

[ApiVersion("1.0")]
public class CreateUserController : UserControllerBase
{
    private readonly IUserService _userService;
    private readonly IUserPresentationFactory _presentationFactory;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public CreateUserController(
        IUserService userService,
        IUserPresentationFactory presentationFactory,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _userService = userService;
        _presentationFactory = presentationFactory;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpPost]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(CancellationToken cancellationToken, CreateUserRequestModel model)
    {
        UserCreateModel createModel = await _presentationFactory.CreateCreationModelAsync(model);

        Attempt<UserCreationResult, UserOperationStatus> result = await _userService.CreateAsync(CurrentUserKey(_backOfficeSecurityAccessor), createModel, true);

        return result.Success
            ? CreatedAtId<ByKeyUserController>(controller => nameof(controller.ByKey), result.Result.CreatedUser!.Key)
            : UserOperationStatusResult(result.Status, result.Result);
    }
}
