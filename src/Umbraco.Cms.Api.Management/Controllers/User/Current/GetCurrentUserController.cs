using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.User.Current;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.User.Current;

[ApiVersion("1.0")]
public class GetCurrentUserController : CurrentUserControllerBase
{
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IUserService _userService;
    private readonly IUserPresentationFactory _userPresentationFactory;

    public GetCurrentUserController(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IUserService userService,
        IUserPresentationFactory userPresentationFactory)
    {
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _userService = userService;
        _userPresentationFactory = userPresentationFactory;
    }

    [MapToApiVersion("1.0")]
    [HttpGet]
    [ProducesResponseType(typeof(CurrentUserResponseModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCurrentUser()
    {
        var currentUserKey = CurrentUserKey(_backOfficeSecurityAccessor);

        IUser? user = await _userService.GetAsync(currentUserKey);

        if (user is null)
        {
            return Unauthorized();
        }

        var responseModel = await _userPresentationFactory.CreateCurrentUserResponseModelAsync(user);
        return Ok(responseModel);
    }
}
