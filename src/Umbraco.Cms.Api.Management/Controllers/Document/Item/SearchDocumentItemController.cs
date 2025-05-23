using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Document.Item;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Document.Item;

[ApiVersion("1.0")]
public class SearchDocumentItemController : DocumentItemControllerBase
{
    private readonly IIndexedEntitySearchService _indexedEntitySearchService;
    private readonly IDocumentPresentationFactory _documentPresentationFactory;

    public SearchDocumentItemController(IIndexedEntitySearchService indexedEntitySearchService, IDocumentPresentationFactory documentPresentationFactory)
    {
        _indexedEntitySearchService = indexedEntitySearchService;
        _documentPresentationFactory = documentPresentationFactory;
    }

    [HttpGet("search")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedModel<DocumentItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchWithTrashed(
        CancellationToken cancellationToken,
        string query,
        bool? trashed = null,
        string? culture = null,
        int skip = 0,
        int take = 100,
        Guid? parentId = null,
        [FromQuery] IEnumerable<Guid>? allowedDocumentTypes = null)
    {
        PagedModel<IEntitySlim> searchResult = await _indexedEntitySearchService.SearchAsync(UmbracoObjectTypes.Document, query, parentId, allowedDocumentTypes, trashed, culture, skip, take);
        var result = new PagedModel<DocumentItemResponseModel>
        {
            Items = searchResult.Items.OfType<IDocumentEntitySlim>().Select(_documentPresentationFactory.CreateItemResponseModel),
            Total = searchResult.Total,
        };

        return Ok(result);
    }
}
