using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.MediaType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.ContentTypeEditing;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.MediaType;

[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessMediaTypes)]
public class UpdateMediaTypeController : MediaTypeControllerBase
{
    private readonly IMediaTypeEditingPresentationFactory _mediaTypeEditingPresentationFactory;
    private readonly IMediaTypeEditingService _mediaTypeEditingService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IMediaTypeService _mediaTypeService;

    public UpdateMediaTypeController(
        IMediaTypeEditingPresentationFactory mediaTypeEditingPresentationFactory,
        IMediaTypeEditingService mediaTypeEditingService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IMediaTypeService mediaTypeService)
    {
        _mediaTypeEditingPresentationFactory = mediaTypeEditingPresentationFactory;
        _mediaTypeEditingService = mediaTypeEditingService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _mediaTypeService = mediaTypeService;
    }

    [HttpPut("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        CancellationToken cancellationToken,
        Guid id,
        UpdateMediaTypeRequestModel requestModel)
    {
        IMediaType? mediaType = await _mediaTypeService.GetAsync(id);
        if (mediaType is null)
        {
            return OperationStatusResult(ContentTypeOperationStatus.NotFound);
        }

        MediaTypeUpdateModel model = _mediaTypeEditingPresentationFactory.MapUpdateModel(requestModel);
        Attempt<IMediaType?, ContentTypeOperationStatus> result = await _mediaTypeEditingService.UpdateAsync(mediaType, model, CurrentUserKey(_backOfficeSecurityAccessor));

        return result.Success
            ? Ok()
            : OperationStatusResult(result.Status);
    }
}
