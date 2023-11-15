using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Sorting;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Media;

[ApiVersion("1.0")]
public class SortMediaController : MediaControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IMediaEditingService _mediaEditingService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public SortMediaController(
        IAuthorizationService authorizationService,
        IMediaEditingService mediaEditingService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _authorizationService = authorizationService;
        _mediaEditingService = mediaEditingService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpPut("sort")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Sort(SortingRequestModel sortingRequestModel)
    {
        AuthorizationResult authorizationResult;

        if (sortingRequestModel.ParentId.HasValue is false)
        {
            authorizationResult = await _authorizationService.AuthorizeAsync(User, $"New{AuthorizationPolicies.MediaPermissionAtRoot}");
        }
        else
        {
            authorizationResult = await _authorizationService.AuthorizeAsync(User, new[] { sortingRequestModel.ParentId.Value },
                $"New{AuthorizationPolicies.MediaPermissionByResource}");
        }

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        ContentEditingOperationStatus result = await _mediaEditingService.SortAsync(
            sortingRequestModel.ParentId,
            sortingRequestModel.Sorting.Select(m => new SortingModel { Key = m.Id, SortOrder = m.SortOrder }),
            CurrentUserKey(_backOfficeSecurityAccessor));

        return result == ContentEditingOperationStatus.Success
            ? Ok()
            : ContentEditingOperationStatusResult(result);
    }
}
