using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.UserData;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Querying;

namespace Umbraco.Cms.Api.Management.Controllers.UserData;

[ApiVersion("1.0")]
public class ByKeyUserDataController : UserDataControllerBase
{
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IUserDataService _userDataService;
    private readonly IUmbracoMapper _umbracoMapper;

    public ByKeyUserDataController(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IUserDataService userDataService,
        IUmbracoMapper umbracoMapper)
    {
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _userDataService = userDataService;
        _umbracoMapper = umbracoMapper;
    }

    [HttpGet("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(UserDataViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ByKey(CancellationToken cancellationToken, Guid id)
    {
        Guid currentUserKey = CurrentUserKey(_backOfficeSecurityAccessor);
        IUserData? data = await _userDataService.GetAsync(id);
        if (data is null)
        {
            return NotFound();
        }

        if (data.UserKey != currentUserKey)
        {
            return Unauthorized();
        }

        return Ok(_umbracoMapper.Map<UserDataResponseModel>(data));
    }
}
