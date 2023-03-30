using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Users;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Users;

public class CreateUsersController : UsersControllerBase
{
    private readonly IUserService _userService;
    private readonly IUserPresentationFactory _presentationFactory;
    private readonly IUmbracoMapper _mapper;

    public CreateUsersController(
        IUserService userService,
        IUserPresentationFactory presentationFactory,
        IUmbracoMapper mapper)
    {
        _userService = userService;
        _presentationFactory = presentationFactory;
        _mapper = mapper;
    }

    [HttpPost]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(CreateUserResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(CreateUserRequestModel model)
    {
        UserCreateModel createModel = await _presentationFactory.CreateCreationModelAsync(model);

        // FIXME: use the actual currently logged in user key
        Attempt<UserCreationResult, UserOperationStatus> result = await _userService.CreateAsync(Constants.Security.SuperUserKey, createModel, true);

        return result.Success
            ? Ok(_mapper.Map<CreateUserResponseModel>(result.Result))
            : UserOperationStatusResult(result.Status, result.Result);
    }
}
