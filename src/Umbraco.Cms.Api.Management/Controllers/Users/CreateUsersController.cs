using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Users;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Users;

public class CreateUsersController : UsersControllerBase
{
    private readonly IUserService _userService;
    private readonly IUserPresentationFactory _presentationFactory;

    public CreateUsersController(
        IUserService userService,
        IUserPresentationFactory presentationFactory)
    {
        _userService = userService;
        _presentationFactory = presentationFactory;
    }

    [HttpPost]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Create(CreateUserRequestModel model)
    {
        UserCreateModel createModel = await _presentationFactory.CreateCreationModelAsync(model);

        Attempt<UserCreationResult, UserOperationStatus> result = await _userService.CreateAsync(-1, createModel, true);

        if (result.Success)
        {
            return Ok(_presentationFactory.CreateCreationResponseModel(result.Result));
        }

        if (result.Status is UserOperationStatus.UnknownFailure)
        {
            return BadRequest(new ProblemDetailsBuilder()
                .WithTitle("An error occured.")
                .WithDetail(result.Result.ErrorMessage ?? "The error was unknown")
                .Build());
        }

        return UserOperationStatusResult(result.Status);
    }
}
