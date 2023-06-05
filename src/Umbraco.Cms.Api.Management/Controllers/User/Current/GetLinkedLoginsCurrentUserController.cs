using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.User;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.User.Current;

[ApiVersion("1.0")]
public class GetLinkedLoginsCurrentUserController : CurrentUserControllerBase
{
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IUserService _userService;
    private readonly IUmbracoMapper _umbracoMapper;

    public GetLinkedLoginsCurrentUserController(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IUserService userService,
        IUmbracoMapper umbracoMapper)
    {
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _userService = userService;
        _umbracoMapper = umbracoMapper;
    }

    [MapToApiVersion("1.0")]
    [HttpGet("logins")]
    [ProducesResponseType(typeof(LinkedLoginsRequestModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLinkedLogins()
    {
        Guid currentUserKey = CurrentUserKey(_backOfficeSecurityAccessor);

        Attempt<ICollection<IIdentityUserLogin>, UserOperationStatus> linkedLoginsAttempt = await _userService.GetLinkedLoginsAsync(currentUserKey);

        if (linkedLoginsAttempt.Success == false)
        {
            return UserOperationStatusResult(linkedLoginsAttempt.Status);
        }

        List<LinkedLoginViewModel> models = _umbracoMapper.MapEnumerable<IIdentityUserLogin, LinkedLoginViewModel>(linkedLoginsAttempt.Result);

        return Ok(new LinkedLoginsRequestModel { LinkedLogins = models });
    }
}
