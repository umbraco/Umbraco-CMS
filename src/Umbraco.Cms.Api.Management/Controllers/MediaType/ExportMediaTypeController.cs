using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.MediaType;

[ApiVersion("1.0")]
public class ExportMediaTypeController : MediaTypeControllerBase
{
    private readonly IMediaTypeService _mediaTypeService;
    private readonly IUdtFileContentFactory _fileContentFactory;

    public ExportMediaTypeController(
        IMediaTypeService mediaTypeService,
        IUdtFileContentFactory fileContentFactory)
    {
        _mediaTypeService = mediaTypeService;
        _fileContentFactory = fileContentFactory;
    }

    [HttpGet("{id:guid}/export")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public IActionResult Export(
        CancellationToken cancellationToken,
        Guid id)
    {
        IMediaType? mediaType = _mediaTypeService.Get(id);
        if (mediaType is null)
        {
            return OperationStatusResult(ContentTypeOperationStatus.NotFound);
        }

        return _fileContentFactory.Create(mediaType);
    }
}
