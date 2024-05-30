using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.UserData;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Querying;

namespace Umbraco.Cms.Api.Management.Controllers.UserData;

[ApiVersion("1.0")]
public class GetUserDataController : UserDataControllerBase
{
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IUserDataService _userDataService;
    private readonly IUmbracoMapper _umbracoMapper;

    public GetUserDataController(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IUserDataService userDataService,
        IUmbracoMapper umbracoMapper)
    {
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _userDataService = userDataService;
        _umbracoMapper = umbracoMapper;
    }

    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<UserDataResponseModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserData(CancellationToken cancellationToken, [FromQuery]string[]? groups, [FromQuery]string[]? identifiers, [FromQuery]int skip = 0, [FromQuery]int take = 100)
    {
        Guid currentUserKey = CurrentUserKey(_backOfficeSecurityAccessor);

        PagedModel<IUserData> data = await _userDataService.GetAsync(
            skip,
            take,
            new UserDataFilter { UserKeys = new[] { currentUserKey }, Groups = groups, Identifiers = identifiers });

        return Ok(new PagedViewModel<UserDataResponseModel>
        {
            Total = data.Total,
            Items = _umbracoMapper.MapEnumerable<IUserData, UserDataResponseModel>(data.Items),
        });
    }
}
