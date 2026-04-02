using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.MediaType;

/// <summary>
/// API controller in Umbraco CMS responsible for exporting media types.
/// </summary>
[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessMediaTypes)]
public class ExportMediaTypeController : MediaTypeControllerBase
{
    private readonly IMediaTypeService _mediaTypeService;
    private readonly IUdtFileContentFactory _fileContentFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExportMediaTypeController"/> class, used for exporting media types.
    /// </summary>
    /// <param name="mediaTypeService">An instance of <see cref="IMediaTypeService"/> used to manage media types.</param>
    /// <param name="fileContentFactory">An instance of <see cref="IUdtFileContentFactory"/> used to generate UDT file content.</param>
    public ExportMediaTypeController(
        IMediaTypeService mediaTypeService,
        IUdtFileContentFactory fileContentFactory)
    {
        _mediaTypeService = mediaTypeService;
        _fileContentFactory = fileContentFactory;
    }

    /// <summary>
    /// Exports the specified media type as a downloadable file.
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier of the media type to export.</param>
    /// <returns>A <see cref="FileContentResult"/> containing the exported media type, or a 404 Not Found result if the media type does not exist.</returns>
    [HttpGet("{id:guid}/export")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Exports a media type.")]
    [EndpointDescription("Exports the media type identified by the provided Id to a downloadable format.")]
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
