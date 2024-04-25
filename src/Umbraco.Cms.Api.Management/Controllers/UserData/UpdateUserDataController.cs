using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.UserData;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.UserData;

[ApiVersion("1.0")]
public class UpdateUserDataController : UserDataControllerBase
{
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IUserDataService _userDataService;
    private readonly IUmbracoMapper _umbracoMapper;

    public UpdateUserDataController(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IUserDataService userDataService,
        IUmbracoMapper umbracoMapper)
    {
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _userDataService = userDataService;
        _umbracoMapper = umbracoMapper;
    }

    [HttpPut]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(UserDataOperationStatus), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(UserDataOperationStatus), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(CancellationToken cancellationToken, UpdateUserDataRequestModel model)
    {
        Guid currentUserKey = CurrentUserKey(_backOfficeSecurityAccessor);

        IUserData userData = _umbracoMapper.Map<IUserData>(model)!;
        userData.UserKey = currentUserKey;

        Attempt<IUserData, UserDataOperationStatus> attempt = await _userDataService.UpdateAsync(userData);

        return attempt.Success
            ? Ok()
            : UserDataOperationStatusResult(attempt.Status);
    }
}
