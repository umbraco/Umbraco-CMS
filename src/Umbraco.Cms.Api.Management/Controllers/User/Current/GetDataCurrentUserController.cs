using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.User.Current;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.User.Current;

[ApiVersion("1.0")]
public class GetDataCurrentUserController : CurrentUserControllerBase
{
    private readonly IUserDataService _userDataService;
    private readonly IUmbracoMapper _mapper;

    public GetDataCurrentUserController(
        IUserDataService userDataService,
        IUmbracoMapper mapper)
    {
        _userDataService = userDataService;
        _mapper = mapper;
    }

    [HttpGet("data")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(UserDataResponseModel), StatusCodes.Status200OK)]
    public Task<IActionResult> GetUserData()
    {
        IEnumerable<UserDataViewModel?> userData = _userDataService.GetUserData().Select(x => _mapper.Map<UserDataViewModel>(x));

        return Task.FromResult<IActionResult>(Ok(new UserDataResponseModel { UserData = userData! }));
    }
}
