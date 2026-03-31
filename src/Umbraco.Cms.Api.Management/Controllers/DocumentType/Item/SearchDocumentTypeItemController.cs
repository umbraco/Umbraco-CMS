using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType.Item;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentType.Item;

/// <summary>
/// Provides API endpoints for searching document type items within the management interface.
/// </summary>
[ApiVersion("1.0")]
public class SearchDocumentTypeItemController : DocumentTypeItemControllerBase
{
    private readonly IUmbracoMapper _mapper;
    private readonly IContentTypeSearchService _contentTypeSearchService;

    /// <summary>
    /// Initializes a new instance of the <see cref="SearchDocumentTypeItemController"/> class.
    /// </summary>
    /// <param name="mapper">An instance of <see cref="IUmbracoMapper"/> used for mapping Umbraco objects.</param>
    /// <param name="contentTypeSearchService">An instance of <see cref="IContentTypeSearchService"/> used to perform content type searches.</param>
    public SearchDocumentTypeItemController(IUmbracoMapper mapper, IContentTypeSearchService contentTypeSearchService)
    {
        _mapper = mapper;
        _contentTypeSearchService = contentTypeSearchService;
    }

    /// <summary>
    /// Searches for document type items matching the specified query, with optional filtering by element type and pagination.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="query">The search query string.</param>
    /// <param name="isElement">Optional filter to include only element document types.</param>
    /// <param name="skip">The number of items to skip (for pagination).</param>
    /// <param name="take">The number of items to return (for pagination).</param>
    /// <returns>A task that represents the asynchronous operation. The result contains an <see cref="IActionResult"/> with a paged list of matching document type items.</returns>
    [HttpGet("search")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedModel<DocumentTypeItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Searches document type items.")]
    [EndpointDescription("Searches document type items by the provided query with pagination support.")]
    public async Task<IActionResult> SearchDocumentType(CancellationToken cancellationToken, string query, bool? isElement = null, int skip = 0, int take = 100)
    {
        PagedModel<IContentType> contentTypes = await _contentTypeSearchService.SearchAsync(query, isElement, cancellationToken, skip, take);
        var result = new PagedModel<DocumentTypeItemResponseModel>
        {
            Items = _mapper.MapEnumerable<IContentType, DocumentTypeItemResponseModel>(contentTypes.Items),
            Total = contentTypes.Total
        };

        return Ok(result);
    }
}
