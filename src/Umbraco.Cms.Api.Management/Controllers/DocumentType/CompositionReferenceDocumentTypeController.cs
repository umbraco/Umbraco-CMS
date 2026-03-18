using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentType;

/// <summary>
/// Provides API endpoints for managing composition references within document types.
/// </summary>
[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessDocumentTypes)]
public class CompositionReferenceDocumentTypeController : DocumentTypeControllerBase
{
    private readonly IContentTypeService _contentTypeService;
    private readonly IUmbracoMapper _umbracoMapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="CompositionReferenceDocumentTypeController"/> class, which manages document type composition references.
    /// </summary>
    /// <param name="contentTypeService">Service used for operations related to content types.</param>
    /// <param name="umbracoMapper">The mapper used to map Umbraco objects to API models.</param>
    public CompositionReferenceDocumentTypeController(IContentTypeService contentTypeService, IUmbracoMapper umbracoMapper)
    {
        _contentTypeService = contentTypeService;
        _umbracoMapper = umbracoMapper;
    }

    /// <summary>
    /// Retrieves all document types that use the specified document type as a composition.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier (GUID) of the document type whose composition references are to be retrieved.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> containing a collection of <see cref="DocumentTypeCompositionResponseModel"/> objects representing document types that reference the specified document type as a composition.
    /// Returns <c>404 Not Found</c> if the document type does not exist.
    /// </returns>
    [HttpGet("{id:guid}/composition-references")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<DocumentTypeCompositionResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Gets composition references.")]
    [EndpointDescription("Gets a collection of document types that reference the specified document type as a composition.")]
    public async Task<IActionResult> CompositionReferences(CancellationToken cancellationToken, Guid id)
    {
        var contentType = await _contentTypeService.GetAsync(id);

        if (contentType is null)
        {
            return OperationStatusResult(ContentTypeOperationStatus.NotFound);
        }

        IEnumerable<IContentType> composedOf = _contentTypeService.GetComposedOf(contentType.Id);
        List<DocumentTypeCompositionResponseModel> responseModels = _umbracoMapper.MapEnumerable<IContentType, DocumentTypeCompositionResponseModel>(composedOf);

        return Ok(responseModels);
    }
}
