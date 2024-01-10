using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.MediaType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.MediaType;

[ApiVersion("1.0")]
public class MoveMediaTypeController : MediaTypeControllerBase
{
    private readonly IMediaTypeService _mediaTypeService;

    public MoveMediaTypeController(IMediaTypeService mediaTypeService)
        => _mediaTypeService = mediaTypeService;

    [HttpPost("{id:guid}/move")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Move(Guid id, MoveMediaTypeRequestModel moveMediaTypeRequestModel)
    {
        IMediaType? source = await _mediaTypeService.GetAsync(id);
        if (source is null)
        {
            return OperationStatusResult(ContentTypeOperationStatus.NotFound);
        }

        Attempt<IMediaType, ContentTypeStructureOperationStatus> result = await _mediaTypeService.MoveAsync(source, moveMediaTypeRequestModel.TargetId);

        return result.Success
            ? Ok()
            : StructureOperationStatusResult(result.Status);
    }
}
