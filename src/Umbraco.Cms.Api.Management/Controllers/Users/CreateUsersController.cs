using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Users;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Users;

public class CreateUsersController : UsersControllerBase
{
    private readonly IUserService _userService;
    private readonly IUmbracoMapper _umbracoMapper;

    public CreateUsersController(
        IUserService userService,
        IUmbracoMapper umbracoMapper)
    {
        _userService = userService;
        _umbracoMapper = umbracoMapper;
    }

    [HttpPost]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Create(CreateUserRequestModel model)
    {
        UserCreateModel? createModel = _umbracoMapper.Map<UserCreateModel>(model);
        throw new NotImplementedException();
    }
}
