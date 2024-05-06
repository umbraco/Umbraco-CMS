using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.UserGroup;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.User.Filter;

[ApiVersion("1.0")]
public class FilterUserGroupFilterController : UserGroupFilterControllerBase
{
    private readonly IUserGroupService _userGroupService;
    private readonly IUserGroupPresentationFactory _userGroupPresentationFactory;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public FilterUserGroupFilterController(
        IUserGroupService userGroupService,
        IUserGroupPresentationFactory userGroupPresentationFactory,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _userGroupService = userGroupService;
        _userGroupPresentationFactory = userGroupPresentationFactory;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<UserGroupResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Filter(
        CancellationToken cancellationToken,
        int skip = 0,
        int take = 100,
        string filter = "")
    {
        Attempt<PagedModel<IUserGroup>, UserGroupOperationStatus> filterAttempt = await _userGroupService.FilterAsync(
            CurrentUserKey(_backOfficeSecurityAccessor),
            filter,
            skip,
            take);

        if (filterAttempt.Success is false)
        {
            return UserGroupOperationStatusResult(filterAttempt.Status);
        }

        IEnumerable<UserGroupResponseModel> viewModels = await _userGroupPresentationFactory.CreateMultipleAsync(filterAttempt.Result.Items);
        var responseModel = new PagedViewModel<UserGroupResponseModel>
        {
            Total = filterAttempt.Result.Total,
            Items = viewModels,
        };

        return Ok(responseModel);
    }
}
