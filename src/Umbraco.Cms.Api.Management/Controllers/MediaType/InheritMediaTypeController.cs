using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.MediaType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.MediaType;

[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessMediaTypes)]
public class InheritMediaTypeController : MediaTypeControllerBase
{
    private readonly IMediaTypeService _mediaTypeService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public InheritMediaTypeController(IMediaTypeService mediaTypeService, IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _mediaTypeService = mediaTypeService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpPut("{id:guid}/inherit")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Move(
        CancellationToken cancellationToken,
        Guid id,
        InheritMediaTypeRequestModel inheritMediaTypeRequestModel)
    {
        Attempt<ContentTypeOperationStatus> result =
            await _mediaTypeService.InheritAsync(
                id,
                inheritMediaTypeRequestModel.Target?.Id,
                CurrentUserKey(_backOfficeSecurityAccessor));

        return result.Success
            ? Ok()
            : OperationStatusResult(result.Result);
    }
}
