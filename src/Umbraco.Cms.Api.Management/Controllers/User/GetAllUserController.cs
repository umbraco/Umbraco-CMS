using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.User;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Exceptions;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Controllers.User;

[ApiVersion("1.0")]
public class GetAllUserController : UserControllerBase
{
    private readonly IUserService _userService;
    private readonly IUserPresentationFactory _userPresentationFactory;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public GetAllUserController(
        IUserService userService,
        IUserPresentationFactory userPresentationFactory,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _userService = userService;
        _userPresentationFactory = userPresentationFactory;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<UserResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken, int skip = 0, int take = 100)
    {
        Attempt<PagedModel<IUser>?, UserOperationStatus> attempt = await _userService.GetAllAsync(CurrentUserKey(_backOfficeSecurityAccessor), skip, take);

        if (attempt.Success is false)
        {
            return UserOperationStatusResult(attempt.Status);
        }

        PagedModel<IUser>? result = attempt.Result;
        if (result is null)
        {
            throw new PanicException("Get all attempt succeeded, but result was null");
        }

        var pagedViewModel = new PagedViewModel<UserResponseModel>
        {
            Total = result.Total,
            Items = result.Items.Select(x => _userPresentationFactory.CreateResponseModel(x))
        };

        return Ok(pagedViewModel);
    }
}
