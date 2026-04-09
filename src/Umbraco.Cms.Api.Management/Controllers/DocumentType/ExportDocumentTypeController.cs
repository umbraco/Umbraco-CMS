using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentType;

/// <summary>
/// API controller responsible for handling the export of document types in the management section.
/// </summary>
[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessDocumentTypes)]
public class ExportDocumentTypeController : DocumentTypeControllerBase
{
    private readonly IContentTypeService _contentTypeService;
    private readonly IUdtFileContentFactory _fileContentFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExportDocumentTypeController"/> class, used for exporting document types.
    /// </summary>
    /// <param name="contentTypeService">The <see cref="IContentTypeService"/> used to manage content types.</param>
    /// <param name="fileContentFactory">The <see cref="IUdtFileContentFactory"/> used to create file content for export.</param>
    public ExportDocumentTypeController(
        IContentTypeService contentTypeService,
        IUdtFileContentFactory fileContentFactory)
    {
        _contentTypeService = contentTypeService;
        _fileContentFactory = fileContentFactory;
    }

    /// <summary>
    /// Exports the specified document type as a downloadable file.
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier of the document type to export.</param>
    /// <returns>A <see cref="FileContentResult"/> containing the exported document type if found; otherwise, a 404 Not Found result.</returns>
    [HttpGet("{id:guid}/export")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Exports a document type.")]
    [EndpointDescription("Exports the document type identified by the provided Id to a downloadable format.")]
    public IActionResult Export(
        CancellationToken cancellationToken,
        Guid id)
    {
        IContentType? contentType = _contentTypeService.Get(id);
        if (contentType is null)
        {
            return OperationStatusResult(ContentTypeOperationStatus.NotFound);
        }

        return _fileContentFactory.Create(contentType);
    }
}
