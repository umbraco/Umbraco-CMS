using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentType;

    /// <summary>
    /// Controller for operations on document types identified by their unique key.
    /// </summary>
[ApiVersion("1.0")]
public class ByKeyDocumentTypeController : DocumentTypeControllerBase
{
    private readonly IContentTypeService _contentTypeService;
    private readonly IUmbracoMapper _umbracoMapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="ByKeyDocumentTypeController"/> class with the specified content type service and Umbraco mapper.
    /// </summary>
    /// <param name="contentTypeService">An instance of <see cref="IContentTypeService"/> used to manage content types.</param>
    /// <param name="umbracoMapper">An instance of <see cref="IUmbracoMapper"/> used for mapping Umbraco objects.</param>
    public ByKeyDocumentTypeController(IContentTypeService contentTypeService, IUmbracoMapper umbracoMapper)
    {
        _contentTypeService = contentTypeService;
        _umbracoMapper = umbracoMapper;
    }

    /// <summary>
    /// Retrieves a document type by its unique identifier.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique <see cref="Guid"/> of the document type to retrieve.</param>
    /// <returns>An <see cref="IActionResult"/> containing the <see cref="DocumentTypeResponseModel"/> if found; otherwise, a not found result.</returns>
    [HttpGet("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(DocumentTypeResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Gets a document type.")]
    [EndpointDescription("Gets a document type identified by the provided Id.")]
    public async Task<IActionResult> ByKey(CancellationToken cancellationToken, Guid id)
    {
        IContentType? contentType = await _contentTypeService.GetAsync(id);
        if (contentType is null)
        {
            return OperationStatusResult(ContentTypeOperationStatus.NotFound);
        }

        DocumentTypeResponseModel model = _umbracoMapper.Map<DocumentTypeResponseModel>(contentType)!;
        return Ok(model);
    }
}
