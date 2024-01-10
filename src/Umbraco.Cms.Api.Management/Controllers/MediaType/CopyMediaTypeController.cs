using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.MediaType;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.MediaType;

[ApiVersion("1.0")]
public class CopyMediaTypeController : MediaTypeControllerBase
{
    private readonly IMediaTypeService _mediaTypeService;

    public CopyMediaTypeController(IMediaTypeService mediaTypeService)
        => _mediaTypeService = mediaTypeService;

    [HttpPost("{id:guid}/copy")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Copy(Guid id, CopyMediaTypeRequestModel copyMediaTypeRequestModel)
    {
        IMediaType? source = await _mediaTypeService.GetAsync(id);
        if (source is null)
        {
            return OperationStatusResult(ContentTypeOperationStatus.NotFound);
        }

        var result = await _mediaTypeService.CopyAsync(source, copyMediaTypeRequestModel.TargetId);

        return result.Success
            ? CreatedAtAction<ByKeyMediaTypeController>(controller => nameof(controller.ByKey), result.Result.Key)
            : StructureOperationStatusResult(result.Status);
    }
}
